using Ardalis.GuardClauses;
using Dapper;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Serialization;
using Melodee.Common.Utility;
using Melodee.Services.Interfaces;
using Melodee.Services.Scanning;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;
using SmartFormat;
using MelodeeModels = Melodee.Common.Models;

namespace Melodee.Services;

public class LibraryService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    SettingService settingService,
    ArtistService artistService,
    AlbumService albumService,
    ISerializer serializer)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    private const string CacheKeyDetailByApiKeyTemplate = "urn:library:apikey:{0}";
    private const string CacheKeyDetailLibraryByType = "urn:library_by_type:{0}";
    private const string CacheKeyDetailTemplate = "urn:library:{0}";

    public async Task<MelodeeModels.OperationResult<Library>> GetInboundLibraryAsync(CancellationToken cancellationToken = default)
    {
        const int libraryType = (int)LibraryType.Inbound;
        var result = await CacheManager.GetAsync(CacheKeyDetailLibraryByType.FormatSmart(libraryType), async () =>
        {
            var library = await LibraryByType(libraryType, cancellationToken);
            if (library == null)
            {
                throw new Exception("Inbound library not found. A Library record must be setup with a type of '1' (Inbound).");
            }

            return library;
        }, cancellationToken).ConfigureAwait(false);
        return new MelodeeModels.OperationResult<Library>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<Library?>> GetByApiKeyAsync(Guid apiKey, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(_ => apiKey == Guid.Empty, apiKey, nameof(apiKey));

        var id = await CacheManager.GetAsync<int?>(CacheKeyDetailByApiKeyTemplate.FormatSmart(apiKey), async () =>
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var dbConn = scopedContext.Database.GetDbConnection();
                return await dbConn
                    .QuerySingleOrDefaultAsync<int?>("SELECT \"Id\" FROM \"Libraries\" WHERE \"ApiKey\" = @apiKey", new { apiKey })
                    .ConfigureAwait(false);
            }
        }, cancellationToken);
        if (id == null)
        {
            return new MelodeeModels.OperationResult<Library?>("Unknown library.")
            {
                Data = null
            };
        }

        return await GetAsync(id.Value, cancellationToken).ConfigureAwait(false);
    }

    public async Task<MelodeeModels.OperationResult<Library?>> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, id, nameof(id));

        var result = await CacheManager.GetAsync(CacheKeyDetailTemplate.FormatSmart(id), async () =>
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                return await scopedContext
                    .Libraries
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                    .ConfigureAwait(false);
            }
        }, cancellationToken);
        return new MelodeeModels.OperationResult<Library?>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<Library>> GetLibraryAsync(CancellationToken cancellationToken = default)
    {
        const int libraryType = (int)LibraryType.Library;
        var result = await CacheManager.GetAsync(CacheKeyDetailLibraryByType.FormatSmart(libraryType), async () =>
        {
            var library = await LibraryByType(libraryType, cancellationToken);
            if (library == null)
            {
                throw new Exception("Library not found. A Library record must be setup with a type of '3' (Library).");
            }

            return library;
        }, cancellationToken);
        return new MelodeeModels.OperationResult<Library>
        {
            Data = result
        };
    }

    private async Task<Library?> LibraryByType(int type, CancellationToken cancellationToken = default)
    {
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbConn = scopedContext.Database.GetDbConnection();
            var sql = $"SELECT * FROM \"Libraries\" WHERE \"Type\" = {type};";
            return await dbConn
                .QuerySingleOrDefaultAsync<Library?>(sql)
                .ConfigureAwait(false);
        }
    }

    public async Task<MelodeeModels.OperationResult<Library?>> PurgeLibraryAsync(int libraryId, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, libraryId, nameof(libraryId));

        var libraryType = (int)LibraryType.Library;
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbLibrary = await scopedContext
                .Libraries
                .FirstOrDefaultAsync(x => x.Id == libraryId, cancellationToken)
                .ConfigureAwait(false);
            if (dbLibrary == null)
            {
                return new MelodeeModels.OperationResult<Library?>("Invalid Library Id")
                {
                    Data = null,
                    Type = MelodeeModels.OperationResponseType.Error,
                };
            }

            libraryType = dbLibrary.Type;
            dbLibrary.PurgePath();

            await scopedContext
                .LibraryScanHistories
                .Where(x => x.LibraryId == libraryId)
                .ExecuteDeleteAsync(cancellationToken)
                .ConfigureAwait(false);

            dbLibrary.LastScanAt = null;
            dbLibrary.LastUpdatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow);
            await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            ClearCache();
        }

        return new MelodeeModels.OperationResult<Library?>
        {
            Data = await LibraryByType(libraryType, cancellationToken).ConfigureAwait(false)
        };
    }

    public async Task<MelodeeModels.OperationResult<Library>> GetStagingLibraryAsync(CancellationToken cancellationToken = default)
    {
        const int libraryType = (int)LibraryType.Staging;
        var result = await CacheManager.GetAsync(CacheKeyDetailLibraryByType.FormatSmart(libraryType), async () =>
        {
            var library = await LibraryByType(libraryType, cancellationToken).ConfigureAwait(false);
            if (library == null)
            {
                throw new Exception("Staging library not found. A Library record must be setup with a type of '2' (Staging).");
            }

            return library;
        }, cancellationToken).ConfigureAwait(false);
        return new MelodeeModels.OperationResult<Library>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.PagedResult<Library>> ListAsync(MelodeeModels.PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        int librariesCount = 0;
        Library[] libraries = [];
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            try
            {
                var orderBy = pagedRequest.OrderByValue();
                var dbConn = scopedContext.Database.GetDbConnection();
                var countSqlParts = pagedRequest.FilterByParts("SELECT COUNT(*) FROM \"Libraries\"");
                librariesCount = await dbConn
                    .QuerySingleAsync<int>(countSqlParts.Item1, countSqlParts.Item2)
                    .ConfigureAwait(false);
                if (!pagedRequest.IsTotalCountOnlyRequest)
                {
                    var listSqlParts = pagedRequest.FilterByParts("SELECT * FROM \"Libraries\"");
                    var listSql = $"{listSqlParts.Item1} ORDER BY {orderBy} OFFSET {pagedRequest.SkipValue} ROWS FETCH NEXT {pagedRequest.TakeValue} ROWS ONLY;";
                    if (dbConn is SqliteConnection)
                    {
                        listSql = $"{listSqlParts.Item1} ORDER BY {orderBy} LIMIT {pagedRequest.TakeValue} OFFSET {pagedRequest.SkipValue};";
                    }

                    libraries = (await dbConn
                        .QueryAsync<Library>(listSql, listSqlParts.Item2)
                        .ConfigureAwait(false)).ToArray();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to get libraries from database");
            }
        }

        return new MelodeeModels.PagedResult<Library>
        {
            TotalCount = librariesCount,
            TotalPages = pagedRequest.TotalPages(librariesCount),
            Data = libraries
        };
    }

    private void ClearCache()
    {
        CacheManager.Remove(CacheKeyDetailLibraryByType.FormatSmart((int)LibraryType.Inbound));
        CacheManager.Remove(CacheKeyDetailLibraryByType.FormatSmart((int)LibraryType.Library));
        CacheManager.Remove(CacheKeyDetailLibraryByType.FormatSmart((int)LibraryType.Staging));
    }

    public async Task<MelodeeModels.OperationResult<bool>> MoveAlbumsToLibrary(Library library, MelodeeModels.Album[] albums, CancellationToken cancellationToken = default)
    {
        // TODO Musicbrainz Db for metadata update job

        bool result = false;
        var configuration = await settingService.GetMelodeeConfigurationAsync(cancellationToken);

        if (albums.Any(x => !x.IsValid(configuration.Configuration)))
        {
            return new MelodeeModels.OperationResult<bool>(albums.Where(x => !x.IsValid(configuration.Configuration)).Select(x => $"Album [{x}] is invalid."))
            {
                Data = false
            };
        }
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var now = Instant.FromDateTimeUtc(DateTime.UtcNow);            
            foreach (var album in albums)
            {
                var albumDirectory = album.AlbumDirectoryName(configuration.Configuration);
                await using (var transaction = await scopedContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false))
                {
                    var artistName = album.Artist() ?? throw new Exception("Album artist is required.");
                    var albumTitle = album.AlbumTitle() ?? throw new Exception("Album title is required.");

                    // See if the artist can be found by the MediaUniqueId
                    var dbArtistResult = await artistService.GetByMediaUniqueId(album.ArtistUniqueId(), cancellationToken).ConfigureAwait(false);

                    // If the artist isn't found by the MediaUniqueId see if it can be found by the NameNormalized value
                    if (!dbArtistResult.IsSuccess)
                    {
                        dbArtistResult = await artistService.GetByNameNormalized(artistName.ToNormalizedString() ?? artistName, cancellationToken).ConfigureAwait(false);
                    }

                    var dbArtist = dbArtistResult.Data;

                    // Artist isn't found proceed to create
                    if (!dbArtistResult.IsSuccess)
                    {
                        dbArtist = new Artist
                        {
                            AlbumCount = 1,
                            CreatedAt = now,
                            MediaUniqueId = album.ArtistUniqueId(),
                            MetaDataStatus = (int)MetaDataModelStatus.ReadyToProcess,
                            Name = artistName,
                            NameNormalized = artistName.ToNormalizedString() ?? artistName,
                            SongCount = album.Songs?.Count() ?? 0,
                            SortName = artistName.CleanString(doPutTheAtEnd: true)
                        };
                        await scopedContext.Artists.AddAsync(dbArtist, cancellationToken).ConfigureAwait(false);
                        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    }

                    // See if the album can be found by the MediaUniqueId
                    var dbAlbumResult = await albumService.GetByMediaUniqueId(album.UniqueId, cancellationToken).ConfigureAwait(false);

                    // If the artist isn't found by the MediaUniqueId see if it can be found by the NameNormalized value
                    if (!dbAlbumResult.IsSuccess)
                    {
                        dbAlbumResult = await albumService.GetByArtistIdAndNameNormalized(dbArtist!.Id, albumTitle.ToNormalizedString() ?? albumTitle, cancellationToken).ConfigureAwait(false);
                    }

                    // Album isn't found for artist proceed to create
                    if (!dbAlbumResult.IsSuccess)
                    {
                        var dbAlbum = new Album
                        {
                            AlbumStatus = (short)album.Status,
                            AlbumType = (int)AlbumType.Album,
                            ArtistId = dbArtist!.Id,
                            CreatedAt = now,
                            Directory = albumDirectory,
                            DiscCount = album.MediaCountValue(),
                            Duration = album.TotalDuration(),
                            Genres = album.Genre() == null ? null : album.Genre()!.Split('/'),
                            IsCompilation = album.IsVariousArtistTypeAlbum(),
                            MediaUniqueId = album.UniqueId,
                            MetaDataStatus = (int)MetaDataModelStatus.ReadyToProcess,
                            Name = albumTitle,
                            NameNormalized = albumTitle.ToNormalizedString() ?? albumTitle,
                            OriginalReleaseDate = album.OriginalAlbumYear() == null ? null : new LocalDate(album.OriginalAlbumYear()!.Value, 1, 1),
                            ReleaseDate = new LocalDate(album.AlbumYear() ?? throw new Exception("Album year is required."), 1, 1),
                            SongCount = SafeParser.ToNumber<short>(album.Songs?.Count() ?? 0),
                            SortName = albumTitle.CleanString(doPutTheAtEnd: true)
                        };
                        await scopedContext.Albums.AddAsync(dbAlbum, cancellationToken).ConfigureAwait(false);
                        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                        var dbAlbumDiscsToAdd = new List<AlbumDisc>();
                        var mediaCountValue = album.MediaCountValue() < 1 ? 1 : album.MediaCountValue();
                        for (short i = 1; i <= mediaCountValue; i++)
                        {
                            dbAlbumDiscsToAdd.Add(new AlbumDisc
                            {
                                AlbumId = dbAlbum.Id,
                                DiscNumber = i,
                                SongCount = SafeParser.ToNumber<short>(album.Songs?.Where(x => x.MediaNumber() == i).Count() ?? 0)
                            });
                        }

                        await scopedContext.AlbumDiscs.AddRangeAsync(dbAlbumDiscsToAdd, cancellationToken).ConfigureAwait(false);
                        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                        var dbSongsToAdd = new List<Song>();
                        foreach (var song in album.Songs!)
                        {
                            var songFileInfo = song.File.ToFileInfo(album.Directory!);
                            var songTitle = song.Title() ?? throw new Exception("Song title is required.");

                            dbSongsToAdd.Add(new Song
                            {
                                AlbumDiscId = dbAlbumDiscsToAdd.First(x => x.DiscNumber == song.MediaNumber()).Id,
                                BitDepth = SafeParser.ToNumber<int>(song.MediaAudios?.FirstOrDefault(x => x.Identifier == MediaAudioIdentifier.BitDepth)?.Value),
                                BitRate = SafeParser.ToNumber<int>(song.MediaAudios?.FirstOrDefault(x => x.Identifier == MediaAudioIdentifier.BitRate)?.Value),
                                BPM = song.MetaTagValue<int>(MetaTagIdentifier.Bpm),
                                ChannelCount = SafeParser.ToNumber<int?>(song.MediaAudios?.FirstOrDefault(x => x.Identifier == MediaAudioIdentifier.Channels)?.Value),
                                CreatedAt = now,
                                Duration = song.Duration() ?? throw new Exception("Song duration is required."),
                                FileHash = Crc32.Calculate(songFileInfo),
                                FileName = songFileInfo.Name,
                                FileSize = songFileInfo.Length,
                                Genres = album.Genre() == null ? null : song.Genre()!.Split('/'),
                                LibraryId = library.Id,
                                Lyrics = song.MetaTagValue<string>(MetaTagIdentifier.UnsynchronisedLyrics) ?? song.MetaTagValue<string>(MetaTagIdentifier.SynchronisedLyrics),
                                MediaUniqueId = song.UniqueId,
                                PartTitles = song.MetaTagValue<string>(MetaTagIdentifier.SubTitle),
                                SamplingRate = SafeParser.ToNumber<int>(song.MediaAudios?.FirstOrDefault(x => x.Identifier == MediaAudioIdentifier.SampleRate)?.Value),
                                SortOrder = song.SortOrder,
                                Title = songTitle,
                                TitleNormalized = songTitle.ToNormalizedString() ?? songTitle,
                                TitleSort = songTitle.CleanString(doPutTheAtEnd: true),
                                SongNumber = song.SongNumber()
                            });
                        }

                        await scopedContext.Songs.AddRangeAsync(dbSongsToAdd, cancellationToken).ConfigureAwait(false);
                        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                        
                        var dbContributorsToAdd = new List<Contributor>();
                        foreach (var song in album.Songs!)
                        {
                            var dbSongId = dbSongsToAdd.First(x => x.MediaUniqueId == song.UniqueId).Id;

                            foreach (var contributorTag in ContributorMetaTagIdentifiers)
                            {
                                var contributorForTag = await CreateContributorForSongAndTag(song, contributorTag, dbArtist.Id, dbAlbum.Id, dbSongId, now, null, cancellationToken);
                                if (contributorForTag != null)
                                {
                                    dbContributorsToAdd.Add(contributorForTag);
                                }
                            }
                            foreach (var tmclTag in song.Tags?.Where(x => x.Value != null && x.Value.ToString()!.StartsWith("TMCL:", StringComparison.InvariantCultureIgnoreCase)) ?? [])
                            {
                                var role = tmclTag.Value.ToString().Substring(6).Trim();
                                var contributorForTag = await CreateContributorForSongAndTag(song, tmclTag.Identifier, dbArtist.Id, dbAlbum.Id, dbSongId, now, role, cancellationToken);
                                if (contributorForTag != null)
                                {
                                    dbContributorsToAdd.Add(contributorForTag);
                                }
                            }
                            // TODO, Publisher (TPUB)
                        }

                        if (dbContributorsToAdd.Count > 0)
                        {
                            Log.Debug("Addedd [{Count}] contributors to album [{Album}].", dbContributorsToAdd.Count, album);
                        }

                        await scopedContext.Contributors.AddRangeAsync(dbContributorsToAdd, cancellationToken).ConfigureAwait(false);

                        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

                        result = true;
                    }
                }
                var libraryAlbumPath = Path.Combine(library.Path, albumDirectory);
                if (!Directory.Exists(libraryAlbumPath))
                {
                    Directory.CreateDirectory(libraryAlbumPath);
                }
                else
                {
                    // if data album exists for model album if so determine which is better quality
                }

                var doMove = SafeParser.ToBoolean(configuration.Configuration[SettingRegistry.ProcessingMoveMelodeeJsonDataFileToLibrary]);
                MediaEditService.MoveDirectory(album.Directory!.FullName(), libraryAlbumPath, doMove ? null : MelodeeModels.Album.JsonFileName);
            }
            return new MelodeeModels.OperationResult<bool>
            {
                Data = result
            };
        }
    }

    private async Task<Contributor?> CreateContributorForSongAndTag(MelodeeModels.Song song,
        MetaTagIdentifier tag,
        int dbArtist,
        int dbAlbumId,
        int dbSongId,
        Instant now,
        string? role,
        CancellationToken cancellationToken = default)
    {
        var tagValue = song.MetaTagValue<string?>(tag);
        if (tagValue != null)
        {
            var artist = await artistService.GetByNameNormalized(tagValue, cancellationToken).ConfigureAwait(false);
            if (artist.IsSuccess)
            {
                var artistContributorId = artist.Data!.Id;
                if (artistContributorId != dbArtist)
                {
                    return new Contributor
                    {
                        CreatedAt = now,
                        Role = role ?? tag.GetEnumDescriptionValue(),
                        ArtistId = artistContributorId,
                        SongId = dbSongId,
                        AlbumId = dbAlbumId
                    };
                }
            }
            else
            {
                Logger.Warning("Unable to find '{Tag}' by name [{Name}]", tag.ToString(), tagValue);
            }
        }

        return null;
    }

    public async Task<MelodeeModels.OperationResult<LibraryScanHistory?>> CreateLibraryScanHistory(Library library, LibraryScanHistory libraryScanHistory, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, library.Id, nameof(library));

        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbLibrary = await scopedContext
                .Libraries
                .FirstOrDefaultAsync(x => x.Id == library.Id, cancellationToken)
                .ConfigureAwait(false);
            if (dbLibrary == null)
            {
                return new MelodeeModels.OperationResult<LibraryScanHistory?>("Invalid Library Id")
                {
                    Data = null,
                    Type = MelodeeModels.OperationResponseType.Error,
                };
            }

            var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
            var newLibraryScanHistory = new LibraryScanHistory
            {
                LibraryId = library.Id,
                CreatedAt = now,
                DurationInMs = libraryScanHistory.DurationInMs,
                ForAlbumId = libraryScanHistory.ForAlbumId,
                ForArtistId = libraryScanHistory.ForArtistId,
                FoundAlbumsCount = libraryScanHistory.FoundAlbumsCount,
                FoundArtistsCount = libraryScanHistory.FoundArtistsCount,
                FoundSongsCount = libraryScanHistory.FoundSongsCount
            };
            scopedContext.LibraryScanHistories.Add(newLibraryScanHistory);
            if (await scopedContext
                    .SaveChangesAsync(cancellationToken)
                    .ConfigureAwait(false) < 1)
            {
                return new MelodeeModels.OperationResult<LibraryScanHistory?>
                {
                    Data = null,
                    Type = MelodeeModels.OperationResponseType.Error
                };
            }

            dbLibrary.LastScanAt = now;
            if (await scopedContext
                    .SaveChangesAsync(cancellationToken)
                    .ConfigureAwait(false) < 1)
            {
                return new MelodeeModels.OperationResult<LibraryScanHistory?>
                {
                    Data = null,
                    Type = MelodeeModels.OperationResponseType.Error
                };
            }

            ClearCache();
            return new MelodeeModels.OperationResult<LibraryScanHistory?>
            {
                Data = newLibraryScanHistory
            };
        }
    }
}
