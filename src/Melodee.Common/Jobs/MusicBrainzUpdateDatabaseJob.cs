using System.Globalization;
using System.Text;
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.Tar;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data;
using Melodee.Common.Services;
using Quartz;
using Serilog;
using Serilog.Events;
using SerilogTimings;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace Melodee.Common.Jobs;

[DisallowConcurrentExecution]
public class MusicBrainzUpdateDatabaseJob(
    ILogger logger,
    IMelodeeConfigurationFactory configurationFactory,
    SettingService settingService,
    IHttpClientFactory httpClientFactory,
    IMusicBrainzRepository repository) : JobBase(logger, configurationFactory)
{
    public override async Task Execute(IJobExecutionContext context)
    {
        Logger.Information("[{JobName}] Starting job.", nameof(MusicBrainzUpdateDatabaseJob));

        var startTicks = Stopwatch.GetTimestamp();
        var configuration = await ConfigurationFactory.GetConfigurationAsync(context.CancellationToken)
            .ConfigureAwait(false);
        if (!configuration.GetValue<bool>(SettingRegistry.SearchEngineMusicBrainzEnabled))
        {
            Logger.Warning("[{JobName}] Search engine music brainz is disabled [{SettingName}], will not run job.",
                nameof(MusicBrainzUpdateDatabaseJob), SettingRegistry.SearchEngineMusicBrainzEnabled);
            return;
        }

        string? storagePath = null;
        string? tempDbName = null;
        var lockfile = string.Empty;
        try
        {
            storagePath = configuration.GetValue<string>(SettingRegistry.SearchEngineMusicBrainzStoragePath);
            if (storagePath == null)
            {
                Logger.Error("[{JobName}] MusicBrainz storage path is invalid [{SettingName}]",
                    nameof(MusicBrainzUpdateDatabaseJob), SettingRegistry.SearchEngineMusicBrainzStoragePath);
                return;
            }

            storagePath.ToFileSystemDirectoryInfo().EnsureExists();

            lockfile = Path.Combine(storagePath, $"{nameof(MusicBrainzUpdateDatabaseJob)}.lock");
            if (File.Exists(lockfile))
            {
                Logger.Warning("[{JobName}] Job lock file found [{LockFile}], will not run job.",
                    nameof(MusicBrainzUpdateDatabaseJob), lockfile);
                return;
            }

            await File.WriteAllTextAsync(lockfile, DateTimeOffset.UtcNow.ToString()).ConfigureAwait(false);

            await settingService
                .SetAsync(SettingRegistry.SearchEngineMusicBrainzEnabled, "false", context.CancellationToken)
                .ConfigureAwait(false);

            var dbName = Path.Combine(storagePath, "musicbrainz.db");
            var doesDbExist = File.Exists(dbName);
            if (doesDbExist)
            {
                // rename musicbrainz.db to something temp if import fails rename back
                tempDbName = Path.Combine(storagePath, $"{Guid.NewGuid()}.db");
                File.Move(dbName, tempDbName);
            }

            // Simple way to skip downloading for debugging
            var doDownload = true;

            using (var client = httpClientFactory.CreateClient())
            {
                var storageStagingDirectory = new FileSystemDirectoryInfo
                {
                    Path = Path.Combine(storagePath, "staging"),
                    Name = "staging"
                };
                if (doDownload)
                {
                    storageStagingDirectory.EnsureExists();
                    storageStagingDirectory.Empty();

                    var latest = await client
                        .GetStringAsync("https://data.metabrainz.org/pub/musicbrainz/data/fullexport/LATEST",
                            context.CancellationToken).ConfigureAwait(false);
                    if (latest.Nullify() == null)
                    {
                        Logger.Error("[{JobName}] Unable to download LATEST information from MusicBrainz",
                            nameof(MusicBrainzUpdateDatabaseJob));
                        return;
                    }

                    latest = latest.CleanString();

                    if (doesDbExist && latest != null)
                    {
                        var latestTimeStamp =
                            DateTimeOffset.ParseExact(latest, "yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
                        var lastJobRunTimestamp =
                            configuration.GetValue<DateTimeOffset?>(SettingRegistry
                                .SearchEngineMusicBrainzImportLastImportTimestamp);
                        if (latestTimeStamp < lastJobRunTimestamp)
                        {
                            Logger.Warning(
                                "[{JobName}] MusicBrainz LATEST is older than Last Job Run timestamp [{SettingName}], meaning latest MusicBrainz export has already been processed.",
                                nameof(MusicBrainzUpdateDatabaseJob),
                                SettingRegistry.SearchEngineMusicBrainzImportLastImportTimestamp);
                            return;
                        }
                    }

                    var mbDumpFileName = Path.Combine(storageStagingDirectory.FullName(), "mbdump.tar.bz2");
                    var downloadedMbDumpFile = await client.DownloadFileAsync(
                        $"https://data.metabrainz.org/pub/musicbrainz/data/fullexport/{latest}/mbdump.tar.bz2",
                        mbDumpFileName,
                        null,
                        context.CancellationToken);

                    var mbDumpDerivedFileName =
                        Path.Combine(storageStagingDirectory.FullName(), "mbdump-derived.tar.bz2");
                    var downloadedMbDerivedFile = await client.DownloadFileAsync(
                        $"https://data.metabrainz.org/pub/musicbrainz/data/fullexport/{latest}/mbdump-derived.tar.bz2",
                        mbDumpDerivedFileName,
                        null,
                        context.CancellationToken);

                    if (!downloadedMbDumpFile || !downloadedMbDerivedFile)
                    {
                        Logger.Warning(
                            "[{JobName}] Unable to download files: mbdump.tar.bz2 [{MbDumpFileName}], mbdump-derived.tar.bz2 [{MbDumpDerivedFileName}]",
                            nameof(MusicBrainzUpdateDatabaseJob),
                            mbDumpFileName,
                            mbDumpDerivedFileName);
                        return;
                    }


                    Logger.Information("[{JobName}] Starting extracted file [{FileName}].",
                        nameof(MusicBrainzUpdateDatabaseJob), mbDumpFileName);
                    using (Operation.At(LogEventLevel.Debug).Time("Extracted downloaded file [{File}]", mbDumpFileName))
                    {
                        await using (Stream mbDumpStream = File.OpenRead(mbDumpFileName))
                        {
                            await using (Stream bzipStream = new BZip2InputStream(mbDumpStream))
                            {
                                var tarArchive = TarArchive.CreateInputTarArchive(bzipStream, Encoding.UTF8);
                                tarArchive.ExtractContents(storageStagingDirectory.FullName());
                                tarArchive.Close();
                                bzipStream.Close();
                            }

                            mbDumpStream.Close();
                        }
                    }


                    Logger.Information("[{JobName}] Starting extracted file [{FileName}].",
                        nameof(MusicBrainzUpdateDatabaseJob), mbDumpDerivedFileName);
                    using (Operation.At(LogEventLevel.Debug)
                               .Time("Extracted downloaded file [{File}]", mbDumpDerivedFileName))
                    {
                        await using (Stream mbDumpDerivedStream = File.OpenRead(mbDumpDerivedFileName))
                        {
                            await using (Stream bzipStream = new BZip2InputStream(mbDumpDerivedStream))
                            {
                                var tarArchive = TarArchive.CreateInputTarArchive(bzipStream, Encoding.UTF8);
                                tarArchive.ExtractContents(storageStagingDirectory.FullName());
                                tarArchive.Close();
                                bzipStream.Close();
                            }

                            mbDumpDerivedStream.Close();
                        }
                    }
                }
            }


            Logger.Information("[{JobName}] Starting importing data.", nameof(MusicBrainzUpdateDatabaseJob));
            var importResult = await repository.ImportData(context.CancellationToken).ConfigureAwait(false);
            if (importResult.IsSuccess)
            {
                if (tempDbName != null)
                {
                    File.Delete(tempDbName);
                }

                await settingService.SetAsync(SettingRegistry.SearchEngineMusicBrainzImportLastImportTimestamp,
                    DateTimeOffset.UtcNow.ToString(), context.CancellationToken).ConfigureAwait(false);
            }

            Log.Debug("ℹ️ [{JobName}] Completed in [{ElapsedTime}] minutes.", nameof(MusicBrainzUpdateDatabaseJob),
                Stopwatch.GetElapsedTime(startTicks).TotalMinutes);
        }
        catch (Exception e)
        {
            if (tempDbName != null && storagePath != null)
            {
                File.Move(tempDbName, Path.Combine(storagePath, "musicbrainz.db"));
            }

            Logger.Error(e, "Error updating database");
        }
        finally
        {
            File.Delete(lockfile);
            await settingService
                .SetAsync(SettingRegistry.SearchEngineMusicBrainzEnabled, "true", context.CancellationToken)
                .ConfigureAwait(false);
        }
    }
}
