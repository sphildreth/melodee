using IdSharp.Common.Utils;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Jobs;
using Melodee.Common.MessageBus.Events;
using Melodee.Common.Metadata;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Rebus.Handlers;
using Rebus.Pipeline;
using Serilog;
using Serilog.Events;
using SerilogTimings;


namespace Melodee.Common.MessageBus.EventHandlers;

/// <summary>
/// Rescan an existing album, syncing database records to album files.
/// </summary>
public sealed class AlbumRescanEventHandler(
    ILogger logger,
    ISerializer serializer,
    IMessageContext messageContext,
    MelodeeMetadataMaker melodeeMetadataMaker,
    IMelodeeConfigurationFactory configurationFactory,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    ArtistService artistService,
    AlbumService albumService,
    LibraryService libraryService
) : IHandleMessages<AlbumRescanEvent>
{
    public async Task Handle(AlbumRescanEvent message)
    {
        var cancellationToken = messageContext.IncomingStepContext.Load<CancellationToken>();

        using (Operation.At(LogEventLevel.Debug).Time("[{Name}] Handle [{id}]", nameof(AlbumRescanEventHandler), message.ToString()))
        {
            await using (var scopedContext = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var now = Instant.FromDateTimeUtc(DateTime.UtcNow);

                var dbAlbum = await scopedContext
                    .Albums
                    .Include(x => x.Contributors)
                    .Include(x => x.Artist).ThenInclude(x => x.Library)
                    .Include(x => x.Songs)
                    .FirstOrDefaultAsync(x => x.Id == message.AlbumId, cancellationToken)
                    .ConfigureAwait(false);

                if (dbAlbum == null)
                {
                    logger.Warning("[{Name}] Unable to find album with id [{AlbumId}] in database.", nameof(AlbumRescanEventHandler), message.AlbumId);
                }
                else
                {
                    var albumDirectory = message.AlbumDirectory.ToDirectoryInfo();

                    // Ensure albums directory exists
                    if (!albumDirectory.Exists())
                    {
                        scopedContext.Albums.Remove(dbAlbum);
                        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                        await libraryService.UpdateAggregatesAsync(dbAlbum.Artist.Library.Id, cancellationToken).ConfigureAwait(false);
                        logger.Warning("[{Name}] Album directory [{AlbumDir}] does not exist. Deleted album.", nameof(AlbumRescanEventHandler), message.AlbumDirectory);
                        return;
                    }
                    
                    var processResult = await melodeeMetadataMaker.MakeMetadataFileAsync(message.AlbumDirectory, cancellationToken).ConfigureAwait(false);
                    if (!processResult.IsSuccess)
                    {
                        logger.Warning("[{Name}] Unable to rebuild media in directory [{DirName}].", nameof(AlbumRescanEventHandler), message.AlbumDirectory);
                    }                    

                    var dbUpdatesDone = false;

                    // Ensure all songs on dbAlbum exist
                    foreach (var dbSong in dbAlbum.Songs.ToArray())
                    {
                        var songPath = Path.Combine(message.AlbumDirectory, dbSong.FileName);
                        if (!File.Exists(songPath))
                        {
                            logger.Warning("[{Name}] Removing song [{SongName}] from album [{AlbumName}].",
                                nameof(AlbumRescanEventHandler),
                                dbSong.FileName,
                                dbAlbum.Name);
                            dbAlbum.Songs.Remove(dbSong);
                            dbAlbum.LastUpdatedAt = now;
                            dbUpdatesDone = true;
                        }
                    }

                    var melodeeFileInfo = albumDirectory.AllFileInfos(Models.Album.JsonFileName).FirstOrDefault();
                    if (melodeeFileInfo == null)
                    {
                        logger.Warning("[{Name}] Unable to find album metadata file in directory [{AlbumDir}].",
                            nameof(AlbumRescanEventHandler),
                            message.AlbumDirectory);
                        return;
                    }

                    var melodeeAlbum = await Models.Album.DeserializeAndInitializeAlbumAsync(serializer, melodeeFileInfo.FullName, cancellationToken).ConfigureAwait(false);
                    if (melodeeAlbum == null)
                    {
                        logger.Warning("[{JobName}] Unable to load melodee file [{MelodeeFile}]",
                            nameof(LibraryInsertJob),
                            melodeeAlbum?.ToString() ?? melodeeFileInfo.FullName);
                        return;
                    }

                    var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);

                    var ignorePerformers = MelodeeConfiguration.FromSerializedJsonArrayNormalized(configuration.Configuration[SettingRegistry.ProcessingIgnoredPerformers], serializer);
                    var ignorePublishers = MelodeeConfiguration.FromSerializedJsonArrayNormalized(configuration.Configuration[SettingRegistry.ProcessingIgnoredPublishers], serializer);
                    var ignoreProduction = MelodeeConfiguration.FromSerializedJsonArrayNormalized(configuration.Configuration[SettingRegistry.ProcessingIgnoredProduction], serializer);

                    // Get all songs in directory for album, add any missing, remove any on album not in directory
                    foreach (var mediaFile in albumDirectory.AllMediaTypeFileInfos())
                    {
                        var albumDbSong = dbAlbum.Songs.FirstOrDefault(x => x.FileName == mediaFile.Name);
                        if (albumDbSong == null)
                        {
                            var mediaFileHash = CRC32.Calculate(mediaFile);
                            var melodeeSong = melodeeAlbum.Songs?.FirstOrDefault(x => string.Equals(x.CrcHash, mediaFileHash, StringComparison.OrdinalIgnoreCase));
                            if (melodeeSong == null)
                            {
                                logger.Warning("[{Name}] Unable to find song with hash [{Hash}] in album metadata.",
                                    nameof(AlbumRescanEventHandler),
                                    mediaFileHash);
                                return;
                            }

                            var songTitle = melodeeSong.Title()?.CleanStringAsIs() ?? throw new Exception("Song title is required.");
                            var dbSong = new Song
                            {
                                AlbumId = dbAlbum.Id,
                                ApiKey = melodeeSong.Id,
                                BitDepth = melodeeSong.BitDepth(),
                                BitRate = melodeeSong.BitRate(),
                                BPM = melodeeSong.MetaTagValue<int>(MetaTagIdentifier.Bpm),
                                ContentType = melodeeSong.ContentType(),
                                CreatedAt = now,
                                Duration = melodeeSong.Duration() ?? throw new Exception("Song duration is required."),
                                FileHash = mediaFileHash,
                                FileName = mediaFile.Name,
                                FileSize = mediaFile.Length,
                                SamplingRate = melodeeSong.SamplingRate(),
                                Title = songTitle,
                                TitleNormalized = songTitle.ToNormalizedString() ?? songTitle,
                                SongNumber = melodeeSong.SongNumber(),
                                ChannelCount = melodeeSong.ChannelCount(),
                                Genres = (melodeeSong.Genre()?.Nullify() ?? melodeeAlbum.Genre()?.Nullify())?.Split('/'),
                                IsVbr = melodeeSong.IsVbr(),
                                Lyrics = melodeeSong.MetaTagValue<string>(MetaTagIdentifier.UnsynchronisedLyrics)?.CleanStringAsIs() ?? melodeeSong.MetaTagValue<string>(MetaTagIdentifier.SynchronisedLyrics)?.CleanStringAsIs(),
                                MusicBrainzId = melodeeSong.MetaTagValue<Guid?>(MetaTagIdentifier.MusicBrainzId),
                                PartTitles = melodeeSong.MetaTagValue<string>(MetaTagIdentifier.SubTitle)?.CleanStringAsIs(),
                                SortOrder = melodeeSong.SortOrder,
                                TitleSort = songTitle.CleanString(true)
                            };
                            var contributorsForSong = await melodeeSong.GetContributorsForSong(
                                now,
                                artistService,
                                dbAlbum.ArtistId,
                                dbAlbum.Id,
                                dbSong.Id,
                                ignorePerformers,
                                ignoreProduction,
                                ignorePublishers,
                                cancellationToken);
                            if (contributorsForSong.Length != 0)
                            {
                                var dbContributorsToAdd = new List<Contributor>();
                                foreach (var cfs in contributorsForSong)
                                {
                                    if (!dbContributorsToAdd.Any(x => x.AlbumId == cfs.AlbumId &&
                                                                      (x.ArtistId == cfs.ArtistId || x.ContributorName == cfs.ContributorName) &&
                                                                      x.MetaTagIdentifier == cfs.MetaTagIdentifier))
                                    {
                                        dbContributorsToAdd.Add(cfs);
                                    }
                                }

                                scopedContext.Contributors.AddRange(dbContributorsToAdd);
                            }

                            dbAlbum.Songs.Add(dbSong);
                            logger.Information("[{Name}] Adding song [{SongName}] to album [{AlbumName}].",
                                nameof(AlbumRescanEventHandler),
                                mediaFile.Name,
                                dbAlbum.Name);
                            dbUpdatesDone = true;
                        }
                    }

                    var imageCount = albumDirectory.AllFileImageTypeFileInfos().Count();
                    if (imageCount != dbAlbum.ImageCount)
                    {
                        dbAlbum.ImageCount = imageCount;
                        if (!dbUpdatesDone)
                        {
                            dbAlbum.LastUpdatedAt = now;
                        }
                        dbUpdatesDone = true;
                    }

                    if (dbUpdatesDone)
                    {
                        dbAlbum.SongCount = SafeParser.ToNumber<short>(dbAlbum.Songs.Count);
                        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                        await libraryService.UpdateAggregatesAsync(dbAlbum.Artist.Library.Id, cancellationToken).ConfigureAwait(false);
                        albumService.ClearCache(dbAlbum);
                    }
                }
            }
        }
    }
}
