using System.Text;
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.Tar;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data.Models;
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

        string? storagePath = null;
        string? tempDbName = null;
        Setting? setting = null;
        string lockfile = string.Empty;
        try
        {
            var settingResult = await settingService.GetAsync(SettingRegistry.SearchEngineMusicBrainzEnabled, context.CancellationToken).ConfigureAwait(false);
            setting = settingResult.Data;
            if (setting == null)
            {
                Logger.Error("[{JobName}] unable to get setting for [{SettingName}]", nameof(MusicBrainzUpdateDatabaseJob), SettingRegistry.SearchEngineMusicBrainzEnabled);
                return;
            }

            setting.Value = "false";
            await settingService.UpdateAsync(setting, context.CancellationToken).ConfigureAwait(false);

            storagePath = configuration.GetValue<string>(SettingRegistry.SearchEngineMusicBrainzStoragePath);
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

            var dbName = Path.Combine(storagePath, "musicbrainz.db");
            var doesDbExist = File.Exists(dbName);
            if (doesDbExist)
            {
                // rename musicbrainz.db to something temp if import fails rename back
                tempDbName = Path.Combine(storagePath, $"{Guid.NewGuid()}.db");
                File.Move(dbName, tempDbName);
            }

            //TODO this is here while debugging to skip downloading
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
                            Logger.Warning("[{JobName}] MusicBrainz LATEST is older than Last Job Run timestamp, meaning latest MusicBrainz export has already been processed.", nameof(MusicBrainzUpdateDatabaseJob));
                            return;
                        }
                    }

                    var mbDumpFileName = Path.Combine(storageStagingDirectory.FullName, "mbdump.tar.bz2");
                    await client.DownloadFileAsync(
                        $"https://data.metabrainz.org/pub/musicbrainz/data/fullexport/{latest}/mbdump.tar.bz2",
                        mbDumpFileName,
                        null,
                        context.CancellationToken);

                    var mbDumpDerivedFileName = Path.Combine(storageStagingDirectory.FullName, "mbdump-derived.tar.bz2");
                    await client.DownloadFileAsync(
                        $"https://data.metabrainz.org/pub/musicbrainz/data/fullexport/{latest}/mbdump-derived.tar.bz2",
                        mbDumpDerivedFileName,
                        null,
                        context.CancellationToken);
                    
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
            if (importResult.IsSuccess && tempDbName != null)
            {
                File.Delete(tempDbName);
            }
            Log.Debug("ℹ️ [{JobName}] Completed in [{ElapsedTime}] minutes.", nameof(LibraryProcessJob), Stopwatch.GetElapsedTime(startTicks).TotalMinutes);            
        }
        catch (Exception e)
        {   
            if (tempDbName != null && storagePath != null)
            {
                File.Move(tempDbName, Path.Combine(storagePath, $"melodee.db"));
            }
            Logger.Error(e, "Error updating database");
        }
        finally
        {
            File.Delete(lockfile);
            if (setting != null)
            {
                setting.Value = "true";
                await settingService.UpdateAsync(setting, context.CancellationToken).ConfigureAwait(false);
            }
        }
    }
}
