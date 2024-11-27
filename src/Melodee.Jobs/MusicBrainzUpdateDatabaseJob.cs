using System.Text;
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.Tar;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Extensions;
using Melodee.Plugins.SearchEngine.MusicBrainz.Data;
using Melodee.Services.Interfaces;
using Quartz;
using Serilog;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace Melodee.Jobs;

[DisallowConcurrentExecution]
public class MusicBrainzUpdateDatabaseJob(
    ILogger logger,
    IMelodeeConfigurationFactory configurationFactory,
    ISettingService settingService,
    IHttpClientFactory httpClientFactory,
    MusicBrainzRepository repository) : JobBase(logger, configurationFactory)
{
    public override async Task Execute(IJobExecutionContext context)
    {
        var startTicks = Stopwatch.GetTimestamp();
        var configuration = await ConfigurationFactory.GetConfigurationAsync(context.CancellationToken).ConfigureAwait(false);
        if (!configuration.GetValue<bool>(SettingRegistry.SearchEngineMusicBrainzEnabled))
        {
            Logger.Warning("[{JobName}] Search engine music brainz is disabled [{SettingName}], will not run job.", nameof(MusicBrainzUpdateDatabaseJob), SettingRegistry.SearchEngineMusicBrainzEnabled);
            return;
        }

        string lockfile = string.Empty;
        try
        {
            var settingResult = await settingService.GetAsync(SettingRegistry.SearchEngineMusicBrainzEnabled, context.CancellationToken).ConfigureAwait(false);
            var setting = settingResult.Data;
            if (setting == null)
            {
                Logger.Error("[{JobName}] unable to get setting for [{SettingName}]", nameof(MusicBrainzUpdateDatabaseJob), SettingRegistry.SearchEngineMusicBrainzEnabled);
                return;
            }

            setting.Value = "false";
            await settingService.UpdateAsync(setting, context.CancellationToken).ConfigureAwait(false);

            var storagePath = configuration.GetValue<string>(SettingRegistry.SearchEngineMusicBrainzStoragePath);
            if (storagePath == null || !Directory.Exists(storagePath))
            {
                Logger.Error("[{JobName}] MusicBrainz storage path is invalid [{SettingName}]", nameof(MusicBrainzUpdateDatabaseJob), SettingRegistry.SearchEngineMusicBrainzStoragePath);
                return;
            }

            lockfile = Path.Combine(storagePath, $"{nameof(MusicBrainzUpdateDatabaseJob)}.lock");
            if (File.Exists(lockfile))
            {
                Logger.Warning("[{JobName}] Job lock file found [{LockFile}], will not run job.", nameof(MusicBrainzUpdateDatabaseJob), lockfile);
                return;
            }

            await File.WriteAllTextAsync(lockfile, DateTimeOffset.UtcNow.ToString()).ConfigureAwait(false);

            var tempDbName = Path.Combine(storagePath, $"{Guid.NewGuid()}.db");
            var dbName = Path.Combine(storagePath, "musicbrainz.db");
            var doesDbExist = File.Exists(dbName);
            if (doesDbExist)
            {
                // rename musicbrainz.db to something temp if import fails rename back
                File.Move(dbName, tempDbName);
            }

            var doDownloadNewFiles = false;
            if (doDownloadNewFiles)
            {
                using (var client = httpClientFactory.CreateClient())
                {
                    var storageStagingDirectory = new DirectoryInfo(Path.Combine(storagePath, "staging"));
                    storageStagingDirectory.Empty();

                    var latest = await client.GetStringAsync("https://data.metabrainz.org/pub/musicbrainz/data/fullexport/LATEST", context.CancellationToken).ConfigureAwait(false);
                    if (latest.Nullify() == null)
                    {
                        Logger.Error("[{JobName}] Unable to download LATEST information from MusicBrainz", nameof(MusicBrainzUpdateDatabaseJob));
                        return;
                    }

                    latest = latest.CleanString();

                    if (doesDbExist && latest != null)
                    {
                        var latestTimeStamp = DateTimeOffset.Parse(latest);
                        var lastJobRunTimestamp = configuration.GetValue<DateTimeOffset?>(SettingRegistry.SearchEngineMusicBrainzImportLastImportTimestamp);
                        if (latestTimeStamp < lastJobRunTimestamp)
                        {
                            Logger.Warning("[{JobName}] LATEST is older than Last Job Run timestamp, meaning latest export has already been imported.", nameof(MusicBrainzUpdateDatabaseJob));
                            return;
                        }
                    }

                    var mbDumpFileName = Path.Combine(storageStagingDirectory.FullName, "mbdump.tar.bz2");
                    await using (var stream = await client.GetStreamAsync($"https://data.metabrainz.org/pub/musicbrainz/data/fullexport/{latest}/mbdump.tar.bz2"))
                    {
                        await using (var fs = new FileStream(mbDumpFileName, FileMode.OpenOrCreate))
                        {
                            await stream.CopyToAsync(fs);
                        }
                    }

                    var mbDumpDerivedFileName = Path.Combine(storageStagingDirectory.FullName, "mbdump-derived.tar.bz2");
                    await using (var stream = await client.GetStreamAsync($"https://data.metabrainz.org/pub/musicbrainz/data/fullexport/{latest}/mbdump-derived.tar.bz2"))
                    {
                        await using (var fs = new FileStream(mbDumpDerivedFileName, FileMode.OpenOrCreate))
                        {
                            await stream.CopyToAsync(fs);
                        }
                    }

                    await using (Stream mbDumpStream = File.OpenRead(mbDumpFileName))
                    {
                        await using (Stream bzipStream = new BZip2InputStream(mbDumpStream))
                        {
                            var tarArchive = TarArchive.CreateInputTarArchive(bzipStream, Encoding.UTF8);
                            tarArchive.ExtractContents(storageStagingDirectory.FullName);
                            tarArchive.Close();
                            bzipStream.Close();
                        }

                        mbDumpStream.Close();
                    }

                    await using (Stream mbDumpDerivedStream = File.OpenRead(mbDumpDerivedFileName))
                    {
                        await using (Stream bzipStream = new BZip2InputStream(mbDumpDerivedStream))
                        {
                            var tarArchive = TarArchive.CreateInputTarArchive(bzipStream, Encoding.UTF8);
                            tarArchive.ExtractContents(storageStagingDirectory.FullName);
                            tarArchive.Close();
                            bzipStream.Close();
                        }

                        mbDumpDerivedStream.Close();
                    }
                }
            }

            var importResult = await repository.ImportData(context.CancellationToken).ConfigureAwait(false);
            if (importResult.IsSuccess)
            {
                File.Delete(tempDbName);
            }

            setting.Value = "true";
            await settingService.UpdateAsync(setting, context.CancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Logger.Error(e, "Error updating database");
        }
        finally
        {
            File.Delete(lockfile);
        }
        Log.Debug("ℹ️ [{JobName}] Completed in [{ElapsedTime}]", nameof(LibraryProcessJob), Stopwatch.GetElapsedTime(startTicks));        
    }
}
