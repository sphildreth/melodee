using System.Diagnostics;
using Melodee.Common.Data;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Serialization;
using Melodee.Common.Utility;
using Melodee.Plugins.MetaData.Song;
using Melodee.Plugins.Processor;
using Melodee.Services;
using Melodee.Services.Scanning;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NodaTime;
using Quartz;
using Serilog;

namespace Melodee.Jobs;

/// <summary>
/// Process non staging and inbound libraries and update database with updated metadata.
/// </summary>
[DisallowConcurrentExecution]
public class LibraryProcessJob(
    ILogger logger,
    SettingService settingService,
    LibraryService libraryService,
    ISerializer serializer,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    DirectoryProcessorService directoryProcessorService) : JobBase(logger, settingService)
{
    public override async Task Execute(IJobExecutionContext context)
    {
        var startTicks = Stopwatch.GetTimestamp();
        var configuration = await settingService.GetMelodeeConfigurationAsync(context.CancellationToken).ConfigureAwait(false);
        var libraries = await libraryService.ListAsync(new PagedRequest(), context.CancellationToken).ConfigureAwait(false);
        if (!libraries.IsSuccess)
        {
            Logger.Warning("[{JobName}] Unable to get libraries, skipping processing.", nameof(LibraryProcessJob));
            return;
        }
        await using (var scopedContext = await contextFactory.CreateDbContextAsync(context.CancellationToken).ConfigureAwait(false))
        {
            var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
            var stagingLibrary = await libraryService.GetStagingLibraryAsync(context.CancellationToken).ConfigureAwait(false);
            if (!stagingLibrary.IsSuccess)
            {
                Logger.Warning("[{JobName}] Unable to get staging library, skipping processing.", nameof(LibraryProcessJob));
                return;
            }
            var mediaFilePlugin = new AtlMetaTag(new MetaTagsProcessor(configuration, serializer), configuration);
            foreach (var library in libraries.Data.Where(x => x.TypeValue == LibraryType.Library))
            {
                if (library.IsLocked)
                {
                    Logger.Warning("[{JobName}] Skipped processing locked library [{LibraryName}]", nameof(LibraryProcessJob), library.Name);
                    continue;
                }
                var dirs = new DirectoryInfo(library.Path).GetDirectories("*", SearchOption.AllDirectories);
                var lastScanAt = library.LastScanAt ?? now;
                foreach (var dir in dirs.Where(d => d.LastWriteTime <= lastScanAt.ToDateTimeUtc()).ToArray())
                {
                    var libraryFileSystemDirectoryInfo = library.ToFileSystemDirectoryInfo();
                    var allDirectoryFiles = dir.GetFiles("*", SearchOption.TopDirectoryOnly);
                    var mediaFiles = allDirectoryFiles.Where(x => FileHelper.IsFileMediaType(x.Extension)).ToArray();
                    var dbAlbum = await scopedContext
                        .Albums.Include(x => x.Discs).ThenInclude(x => x.Songs)
                        .FirstOrDefaultAsync(a => a.Directory == dir.Name, context.CancellationToken);
                    if (dbAlbum != null && (allDirectoryFiles.Length == 0 || mediaFiles.Length == 0))
                    {
                        // Albums directory is empty or has no media files, delete album, delete folder.
                        await scopedContext
                            .Libraries
                            .Where(x => x.Id == library.Id)
                            .ExecuteDeleteAsync(context.CancellationToken)
                            .ConfigureAwait(false);
                        await scopedContext.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
                        dir.Delete(true);
                        Logger.Warning("[{JobName}] Deleted empty album directory [{DirName}]", nameof(LibraryProcessJob), dir.Name);
                        continue;
                    }
                    if (dbAlbum == null)
                    {
                        MediaEditService.MoveDirectory(dir.FullName, stagingLibrary.Data.Path, null);
                        Logger.Debug("[{JobName}] Moved album directory [{DirName}] to staging library.", nameof(LibraryProcessJob),dir.FullName);
                        continue;
                    }
                    var foundSongsMetaTagResults = new List<Song>();
                    var dbSongsToAdd = new List<Common.Data.Models.Song>();
                    foreach (var mediaFile in mediaFiles)
                    {
                        var dbSong = dbAlbum.Discs.SelectMany(x => x.Songs).FirstOrDefault(x => x.FileName == mediaFile.Name);
                        var songMetaTagResult = await mediaFilePlugin.ProcessFileAsync(libraryFileSystemDirectoryInfo, mediaFile.ToFileSystemInfo(), context.CancellationToken).ConfigureAwait(false);
                        if (!songMetaTagResult.IsSuccess)
                        {
                            Logger.Warning("[{JobName}] failed to load metadata for file [{FileName}].", nameof(LibraryProcessJob), mediaFile.Name); 
                            continue;
                        }
                        var song = songMetaTagResult.Data;
                        foundSongsMetaTagResults.Add(song);
                        if (dbSong != null)
                        {
                            // - Insert any missing songs found
                            var songTitle = song.Title();
                            if (songTitle.Nullify() == null)
                            {
                                Logger.Warning("[{JobName}] unable to add song [{SongName}] Song is missing Title.", nameof(LibraryProcessJob), mediaFile.FullName);
                                continue;
                            }
                            dbSongsToAdd.Add(new Common.Data.Models.Song
                            {
                                AlbumDiscId = dbAlbum.Discs.First(x => x.DiscNumber == song.MediaNumber()).Id,
                                BitDepth = SafeParser.ToNumber<int>(song.MediaAudios?.FirstOrDefault(x => x.Identifier == MediaAudioIdentifier.BitDepth)?.Value),
                                BitRate = SafeParser.ToNumber<int>(song.MediaAudios?.FirstOrDefault(x => x.Identifier == MediaAudioIdentifier.BitRate)?.Value),
                                BPM = song.MetaTagValue<int>(MetaTagIdentifier.Bpm),
                                ChannelCount = SafeParser.ToNumber<int?>(song.MediaAudios?.FirstOrDefault(x => x.Identifier == MediaAudioIdentifier.Channels)?.Value),
                                CreatedAt = now,
                                Duration = song.Duration() ?? throw new Exception("Song duration is required."),
                                FileHash = Crc32.Calculate(mediaFile),
                                FileName = mediaFile.Name,
                                FileSize = mediaFile.Length,
                                Genres = dbAlbum.Genres?.Length < 1 ? null : song.Genre()!.Split('/'),
                                Lyrics = song.MetaTagValue<string>(MetaTagIdentifier.UnsynchronisedLyrics) ?? song.MetaTagValue<string>(MetaTagIdentifier.SynchronisedLyrics),
                                MediaUniqueId = song.UniqueId,
                                PartTitles = song.MetaTagValue<string>(MetaTagIdentifier.SubTitle),
                                SamplingRate = SafeParser.ToNumber<int>(song.MediaAudios?.FirstOrDefault(x => x.Identifier == MediaAudioIdentifier.SampleRate)?.Value),
                                SortOrder = song.SortOrder,
                                Title = songTitle!,
                                TitleNormalized = songTitle!.ToNormalizedString() ?? songTitle!,
                                TitleSort = songTitle!.CleanString(true),
                                SongNumber = song.SongNumber()
                            });
                        }
                    }
                    if (dbSongsToAdd.Count != 0)
                    {
                        await scopedContext.Songs.AddRangeAsync(dbSongsToAdd, context.CancellationToken).ConfigureAwait(false);
                        await scopedContext.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
                    }
                    // - Delete any songs not found in directory but in database
                    
                    // - Update album metadata from found songs
                    // - Update album totals
                    // - Update library totals
                }
            }

            throw new NotImplementedException();
        }
    }
}
