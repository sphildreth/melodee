using IdSharp.Common.Utils;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
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
/// Add a new album from album files.
/// </summary>
public sealed class AlbumAddEventHandler(
    ILogger logger,
    ISerializer serializer,
    IMessageContext messageContext,
    MelodeeMetadataMaker melodeeMetadataMaker,
    IMelodeeConfigurationFactory configurationFactory,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    ArtistService artistService,
    LibraryService libraryService
) : IHandleMessages<AlbumAddEvent>
{
    public async Task Handle(AlbumAddEvent message)
    {
        var cancellationToken = messageContext.IncomingStepContext.Load<CancellationToken>();

        using (Operation.At(LogEventLevel.Debug).Time("[{Name}] Handle [{id}]", nameof(AlbumAddEventHandler), message.ToString()))
        {
            await using (var scopedContext = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
                var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);

                var ignorePerformers = MelodeeConfiguration.FromSerializedJsonArrayNormalized(configuration.Configuration[SettingRegistry.ProcessingIgnoredPerformers], serializer);
                var ignorePublishers = MelodeeConfiguration.FromSerializedJsonArrayNormalized(configuration.Configuration[SettingRegistry.ProcessingIgnoredPublishers], serializer);
                var ignoreProduction = MelodeeConfiguration.FromSerializedJsonArrayNormalized(configuration.Configuration[SettingRegistry.ProcessingIgnoredProduction], serializer);

                var dbArtist = await scopedContext
                    .Artists
                    .Include(x => x.Library)
                    .Include(x => x.Contributors)
                    .Include(x => x.Albums)
                    .FirstOrDefaultAsync(x => x.Id == message.ArtistId, cancellationToken)
                    .ConfigureAwait(false);

                if (dbArtist == null)
                {
                    logger.Warning("[{Name}] Unable to find artist with id [{ArtistId}] in database.", nameof(AlbumAddEventHandler), message.ArtistId);
                }
                else
                {
                    if (dbArtist.IsLocked || dbArtist.Library.IsLocked)
                    {
                        logger.Warning("[{Name}] Library or artist is locked. Skipping rescan request [{ArtistDir}].",
                            nameof(AlbumRescanEventHandler),
                            message.AlbumDirectory);
                        return;
                    }

                    var processResult = await melodeeMetadataMaker.MakeMetadataFileAsync(message.AlbumDirectory, false, cancellationToken).ConfigureAwait(false);
                    if (!processResult.IsSuccess || processResult.Data == null)
                    {
                        logger.Warning("[{Name}] Unable to rebuild media in directory [{DirName}].", nameof(AlbumAddEventHandler), message.AlbumDirectory);
                    }

                    var melodeeAlbum = processResult.Data!;

                    var albumTitle = melodeeAlbum.AlbumTitle()?.CleanStringAsIs() ?? throw new Exception("Album title is required.");
                    var nameNormalized = albumTitle.ToNormalizedString() ?? albumTitle;
                    if (nameNormalized.Nullify() == null)
                    {
                        logger.Warning("Album [{Album}] has invalid Album title, unable to generate NameNormalized.", melodeeAlbum);
                        return;
                    }
                    var newAlbum = new Album
                    {
                        AlbumStatus = (short)melodeeAlbum.Status,
                        AlbumType = SafeParser.ToNumber<short>(melodeeAlbum.AlbumType),
                        AmgId = melodeeAlbum.AmgId,
                        ApiKey = melodeeAlbum.Id,
                        Artist = dbArtist,
                        CreatedAt = now,
                        Directory = melodeeAlbum.AlbumDirectoryName(configuration.Configuration),
                        DiscogsId = melodeeAlbum.DiscogsId,
                        Duration = melodeeAlbum.TotalDuration(),
                        Genres = melodeeAlbum.Genre() == null ? null : melodeeAlbum.Genre()!.Split('/'),
                        ImageCount = melodeeAlbum.Images?.Count(),
                        IsCompilation = melodeeAlbum.IsVariousArtistTypeAlbum(),
                        ItunesId = melodeeAlbum.ItunesId,
                        LastFmId = melodeeAlbum.LastFmId,
                        MetaDataStatus = (int)MetaDataModelStatus.ReadyToProcess,
                        MusicBrainzId = SafeParser.ToGuid(melodeeAlbum.MusicBrainzId),
                        Name = albumTitle,
                        NameNormalized = nameNormalized,
                        OriginalReleaseDate = melodeeAlbum.OriginalAlbumYear() == null ? null : SafeParser.ToLocalDate(melodeeAlbum.OriginalAlbumYear()!.Value),
                        ReleaseDate = SafeParser.ToLocalDate(melodeeAlbum.AlbumYear() ?? throw new Exception("Album year is required.")),
                        SongCount = SafeParser.ToNumber<short>(melodeeAlbum.Songs?.Count() ?? 0),
                        SortName = configuration.RemoveUnwantedArticles(albumTitle.CleanString(true)),
                        SpotifyId = melodeeAlbum.SpotifyId,
                        WikiDataId = melodeeAlbum.WikiDataId
                    };
                    if (dbArtist.Albums.Any(x => x.NameNormalized == nameNormalized || 
                                                 (x.MusicBrainzId != null && x.MusicBrainzId == newAlbum.MusicBrainzId) || 
                                                 (x.SpotifyId != null && x.SpotifyId == newAlbum.SpotifyId)))
                    {
                        logger.Warning("For artist [{Artist}] found duplicate album [{Album}]", dbArtist, newAlbum);
                        melodeeAlbum.Directory.AppendPrefix(configuration.GetValue<string>(SettingRegistry.ProcessingDuplicateAlbumPrefix) ?? "__duplicate_ ");
                        return;
                    }

                    logger.Debug("[{JobName}] Creating new album for ArtistId [{ArtistId}] Id [{Id}] NormalizedName [{Name}] Directory [{Directory}]",
                        nameof(AlbumRescanEventHandler),
                        dbArtist.Id,
                        melodeeAlbum.Id,
                        nameNormalized,
                        melodeeAlbum.Directory.FullName());

                    var newAlbumSongs = new List<Song>();
                    foreach (var song in melodeeAlbum.Songs ?? [])
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            newAlbumSongs.Clear();
                            break;
                        }

                        var mediaFile = song.File.ToFileInfo(melodeeAlbum.Directory);
                        if (!mediaFile.Exists)
                        {
                            logger.Warning("[{JobName}] Unable to find media file [{FileName}].",
                                nameof(AlbumRescanEventHandler),
                                mediaFile.FullName);
                            return;
                        }

                        var mediaFileHash = CRC32.Calculate(mediaFile);
                        var songTitle = song.Title()?.CleanStringAsIs() ?? throw new Exception("Song title is required.");
                        var s = new Song
                        {
                            AlbumId = newAlbum.Id,
                            ApiKey = song.Id,
                            BitDepth = song.BitDepth(),
                            BitRate = song.BitRate(),
                            BPM = song.MetaTagValue<int>(MetaTagIdentifier.Bpm),
                            ContentType = song.ContentType(),
                            CreatedAt = now,
                            Duration = song.Duration() ?? throw new Exception("Song duration is required."),
                            FileHash = mediaFileHash,
                            FileName = mediaFile.Name,
                            FileSize = mediaFile.Length,
                            SamplingRate = song.SamplingRate(),
                            Title = songTitle,
                            TitleNormalized = songTitle.ToNormalizedString() ?? songTitle,
                            SongNumber = song.SongNumber(),
                            ChannelCount = song.ChannelCount(),
                            Genres = (song.Genre()?.Nullify() ?? melodeeAlbum.Genre()?.Nullify())?.Split('/'),
                            IsVbr = song.IsVbr(),
                            Lyrics = song.MetaTagValue<string>(MetaTagIdentifier.UnsynchronisedLyrics)?.CleanStringAsIs() ?? song.MetaTagValue<string>(MetaTagIdentifier.SynchronisedLyrics)?.CleanStringAsIs(),
                            MusicBrainzId = song.MetaTagValue<Guid?>(MetaTagIdentifier.MusicBrainzId),
                            PartTitles = song.MetaTagValue<string>(MetaTagIdentifier.SubTitle)?.CleanStringAsIs(),
                            SortOrder = song.SortOrder,
                            TitleSort = songTitle.CleanString(true)
                        };
                        newAlbumSongs.Add(s);
                    }

                    if (newAlbumSongs.Any())
                    {
                        newAlbum.Songs = newAlbumSongs;

                        try
                        {
                            newAlbum.SongCount = SafeParser.ToNumber<short>(newAlbumSongs.Count);
                            scopedContext.Albums.Add(newAlbum);
                            await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                            var dbContributorsToAdd = new List<Contributor>();
                            foreach (var song in melodeeAlbum.Songs ?? [])
                            {
                                var dbSong = newAlbum.Songs.FirstOrDefault(x => x.ApiKey == song.Id);
                                if (dbSong != null)
                                {
                                    var contributorsForSong = await song.GetContributorsForSong(
                                        now,
                                        artistService,
                                        newAlbum.ArtistId,
                                        newAlbum.Id,
                                        dbSong.Id,
                                        ignorePerformers,
                                        ignoreProduction,
                                        ignorePublishers,
                                        cancellationToken);
                                    foreach (var cfs in contributorsForSong)
                                    {
                                        if (!dbContributorsToAdd.Any(x => x.AlbumId == cfs.AlbumId &&
                                                                          (x.ArtistId == cfs.ArtistId || x.ContributorName == cfs.ContributorName) &&
                                                                          x.MetaTagIdentifier == cfs.MetaTagIdentifier))
                                        {
                                            dbContributorsToAdd.Add(cfs);
                                        }
                                    }
                                }
                            }

                            if (dbContributorsToAdd.Count > 0)
                            {
                                try
                                {
                                    await scopedContext.Contributors.AddRangeAsync(dbContributorsToAdd, cancellationToken).ConfigureAwait(false);
                                    await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                                }
                                catch (Exception e)
                                {
                                    logger.Error(e, "Unable to insert album contributors into db.");
                                }
                            }

                            if (!message.IsFromArtistScan)
                            {
                                await libraryService.UpdateAggregatesAsync(newAlbum.Artist.Library.Id, cancellationToken).ConfigureAwait(false);
                                artistService.ClearCache(newAlbum.Artist);
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error(e, "Unable to insert albums into db.");
                        }
                    }
                }
            }
        }
    }
}
