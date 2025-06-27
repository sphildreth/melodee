using Ardalis.GuardClauses;
using Dapper;
using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.MessageBus.Events;
using Melodee.Common.Models.Collection;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Serialization;
using Melodee.Common.Services.Caching;
using Melodee.Common.Services.Scanning;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Rebus.Bus;
using Serilog;
using SmartFormat;
using MelodeeModels = Melodee.Common.Models;

namespace Melodee.Common.Services;

public class AlbumService(
    ILogger logger,
    ICacheManager cacheManager,
    IMelodeeConfigurationFactory configurationFactory,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    IBus bus,
    ISerializer serializer,
    MediaEditService mediaEditService)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    private const string CacheKeyDetailByApiKeyTemplate = "urn:album:apikey:{0}";
    private const string CacheKeyDetailByNameNormalizedTemplate = "urn:album:namenormalized:{0}";
    private const string CacheKeyDetailByMusicBrainzIdTemplate = "urn:album:musicbrainzid:{0}";
    private const string CacheKeyDetailTemplate = "urn:album:{0}";

    public async Task ClearCacheForArtist(int artistId, CancellationToken cancellationToken = default)
    {
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            // Get the artist from db context
            var dbArtist = await scopedContext
                .Artists
                .Include(x => x.Albums)
                .FirstOrDefaultAsync(x => x.Id == artistId, cancellationToken).ConfigureAwait(false);

            // For each album for artist clear the cache for the artist
            foreach (var album in dbArtist?.Albums ?? [])
            {
                await ClearCacheAsync(album.Id, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    public async Task ClearCacheAsync(int albumId, CancellationToken cancellationToken = default)
    {
        var album = await GetAsync(albumId, cancellationToken).ConfigureAwait(false);
        if (album.Data != null)
        {
            CacheManager.Remove(CacheKeyDetailByApiKeyTemplate.FormatSmart(album.Data.ApiKey));
            CacheManager.Remove(CacheKeyDetailByNameNormalizedTemplate.FormatSmart(album.Data.NameNormalized));
            CacheManager.Remove(CacheKeyDetailTemplate.FormatSmart(album.Data.Id));
            if (album.Data.MusicBrainzId != null)
            {
                CacheManager.Remove(
                    CacheKeyDetailByMusicBrainzIdTemplate.FormatSmart(album.Data.MusicBrainzId.Value.ToString()));
            }
        }
    }

    public async Task<MelodeeModels.PagedResult<AlbumDataInfo>> ListForContributorsAsync(
        MelodeeModels.PagedRequest pagedRequest, string contributorName, CancellationToken cancellationToken = default)
    {
        int albumCount;
        AlbumDataInfo[] albums = [];
        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbConn = scopedContext.Database.GetDbConnection();

            var sql = """
                      SELECT COUNT(a.*)
                      from "Contributors" c
                      join "Albums" a on (a."Id" = c."AlbumId")
                      where (c."ContributorName" ILIKE '%{0}%');
                      """.FormatSmart(contributorName);

            albumCount = await dbConn.ExecuteScalarAsync<int>(sql);

            if (!pagedRequest.IsTotalCountOnlyRequest)
            {
                sql = """
                      SELECT a."Id", a."ApiKey", a."IsLocked", a."Name", a."NameNormalized", a."AlternateNames",
                        ar."ApiKey" as "ArtistApiKey", ar."Name" as "ArtistName",
                        a."SongCount", a."Duration", a."CreatedAt", a."Tags", a."ReleaseDate", 
                        a."AlbumStatus"
                      from "Contributors" c
                      join "Albums" a on (a."Id" = c."AlbumId")
                      JOIN "Artists" ar ON (a."ArtistId" = ar."Id")
                      where (c."ContributorName" ILIKE '%{0}%')
                      ORDER BY a.{1} OFFSET {2} ROWS FETCH NEXT {3} ROWS only;
                      """.FormatSmart(contributorName, pagedRequest.OrderByValue(), pagedRequest.SkipValue,
                    pagedRequest.TakeValue);
                albums = (await dbConn
                    .QueryAsync<AlbumDataInfo>(sql)
                    .ConfigureAwait(false)).ToArray();
            }
        }

        return new MelodeeModels.PagedResult<AlbumDataInfo>
        {
            TotalCount = albumCount,
            TotalPages = pagedRequest.TotalPages(albumCount),
            Data = albums
        };
    }

    public async Task<MelodeeModels.PagedResult<AlbumDataInfo>> ListForArtistApiKeyAsync(
        MelodeeModels.PagedRequest pagedRequest, Guid filterToArtistApiKey,
        CancellationToken cancellationToken = default)
    {
        int albumCount;
        AlbumDataInfo[] albums = [];
        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            albumCount = await scopedContext
                .Albums
                .CountAsync(x => x.Artist.ApiKey == filterToArtistApiKey, cancellationToken)
                .ConfigureAwait(false);
            if (!pagedRequest.IsTotalCountOnlyRequest)
            {
                var dbConn = scopedContext.Database.GetDbConnection();
                var filterBy = pagedRequest.FilterBy?.FirstOrDefault(x => x.PropertyName == "Name")?.Value.ToString()
                    .ToNormalizedString();
                var sql = """
                          SELECT a."Id", a."ApiKey", a."IsLocked", a."Name", a."NameNormalized", a."AlternateNames",
                            ar."ApiKey" as "ArtistApiKey", ar."Name" as "ArtistName",
                            a."SongCount", a."Duration", a."CreatedAt", a."Tags", a."ReleaseDate", 
                            a."AlbumStatus"
                          FROM "Albums" a
                          JOIN "Artists" ar ON (a."ArtistId" = ar."Id")
                          where ar."ApiKey" = '{0}'
                          and ('{1}' = '' or a."NameNormalized" LIKE '%{1}%')
                          ORDER BY a.{2} OFFSET {3} ROWS FETCH NEXT {4} ROWS only;
                          """.FormatSmart(filterToArtistApiKey.ToString(), filterBy ?? string.Empty,
                    pagedRequest.OrderByValue(), pagedRequest.SkipValue, pagedRequest.TakeValue);
                albums = (await dbConn
                    .QueryAsync<AlbumDataInfo>(sql)
                    .ConfigureAwait(false)).ToArray();
            }
        }

        return new MelodeeModels.PagedResult<AlbumDataInfo>
        {
            TotalCount = albumCount,
            TotalPages = pagedRequest.TotalPages(albumCount),
            Data = albums
        };
    }

    public async Task<MelodeeModels.PagedResult<AlbumDataInfo>> ListAsync(MelodeeModels.PagedRequest pagedRequest,
        string? filteringColumnNamePrefix = null, CancellationToken cancellationToken = default)
    {
        int albumCount;
        AlbumDataInfo[] albums = [];
        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var orderBy = pagedRequest.OrderByValue();
            var dbConn = scopedContext.Database.GetDbConnection();
            var countSqlParts = pagedRequest.FilterByParts(
                "SELECT COUNT(a.*) FROM \"Albums\" a JOIN \"Artists\" ar ON (a.\"ArtistId\" = ar.\"Id\")  ",
                filteringColumnNamePrefix ?? "a");
            albumCount = await dbConn
                .QuerySingleAsync<int>(countSqlParts.Item1, countSqlParts.Item2)
                .ConfigureAwait(false);
            if (!pagedRequest.IsTotalCountOnlyRequest)
            {
                var sqlStartFragment = """
                                       SELECT a."Id", a."ApiKey", a."IsLocked", a."Name", a."NameNormalized", a."AlternateNames",
                                              ar."ApiKey" as "ArtistApiKey", ar."Name" as "ArtistName",
                                              a."SongCount", a."Duration", a."CreatedAt", a."Tags", a."ReleaseDate", 
                                              a."AlbumStatus"
                                       FROM "Albums" a
                                       JOIN "Artists" ar ON (a."ArtistId" = ar."Id")
                                       """;
                var listSqlParts = pagedRequest.FilterByParts(sqlStartFragment, filteringColumnNamePrefix ?? "a");
                var listSql =
                    $"{listSqlParts.Item1} ORDER BY {orderBy} OFFSET {pagedRequest.SkipValue} ROWS FETCH NEXT {pagedRequest.TakeValue} ROWS ONLY;";
                albums = (await dbConn
                    .QueryAsync<AlbumDataInfo>(listSql, listSqlParts.Item2)
                    .ConfigureAwait(false)).ToArray();
            }
        }

        return new MelodeeModels.PagedResult<AlbumDataInfo>
        {
            TotalCount = albumCount,
            TotalPages = pagedRequest.TotalPages(albumCount),
            Data = albums
        };
    }


    public async Task<MelodeeModels.OperationResult<bool>> DeleteAsync(int[] albumIds,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrEmpty(albumIds, nameof(albumIds));

        bool result;

        var artistIds = new List<int>();
        var libraryIds = new List<int>();

        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            foreach (var albumId in albumIds)
            {
                var artist = await GetAsync(albumId, cancellationToken).ConfigureAwait(false);
                if (!artist.IsSuccess)
                {
                    return new MelodeeModels.OperationResult<bool>("Unknown album.")
                    {
                        Data = false
                    };
                }
            }

            foreach (var albuMid in albumIds)
            {
                var album = await scopedContext
                    .Albums.Include(x => x.Artist).ThenInclude(x => x.Library)
                    .FirstAsync(x => x.Id == albuMid, cancellationToken)
                    .ConfigureAwait(false);

                var albumDirectory = Path.Combine(album.Artist.Library.Path, album.Artist.Directory, album.Directory);
                if (Directory.Exists(albumDirectory))
                {
                    Directory.Delete(albumDirectory, true);
                }

                scopedContext.Albums.Remove(album);
                artistIds.Add(album.ArtistId);
                libraryIds.Add(album.Artist.LibraryId);
            }

            await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            foreach (var artistId in artistIds.Distinct())
            {
                await UpdateArtistAggregateValuesByIdAsync(artistId, cancellationToken).ConfigureAwait(false);
            }

            foreach (var libraryId in libraryIds.Distinct())
            {
                await UpdateLibraryAggregateStatsByIdAsync(libraryId, cancellationToken).ConfigureAwait(false);
            }

            Logger.Information("Deleted albums [{AlbumIds}].", albumIds);
            result = true;
        }

        return new MelodeeModels.OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<Album?>> GetByArtistIdAndNameNormalized(int artistId,
        string nameNormalized, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrEmpty(nameNormalized, nameof(nameNormalized));

        int? id = null;
        try
        {
            id = await CacheManager.GetAsync(CacheKeyDetailByNameNormalizedTemplate.FormatSmart(nameNormalized),
                async () =>
                {
                    await using (var scopedContext =
                                 await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
                    {
                        var dbConn = scopedContext.Database.GetDbConnection();
                        return await dbConn
                            .QuerySingleOrDefaultAsync<int?>(
                                "SELECT \"Id\" FROM \"Albums\" WHERE \"ArtistId\" = @artistId AND \"NameNormalized\" = @nameNormalized",
                                new { artistId, nameNormalized })
                            .ConfigureAwait(false);
                    }
                }, cancellationToken);
        }
        catch (Exception e)
        {
            Logger.Error(e, "Failed to get album [{Name}] for artistId [{ArtistId}].", nameNormalized, artistId);
        }

        if (id == null)
        {
            return new MelodeeModels.OperationResult<Album?>("Unknown album.")
            {
                Data = null
            };
        }

        return await GetAsync(id.Value, cancellationToken).ConfigureAwait(false);
    }

    public async Task<MelodeeModels.OperationResult<Album?>> GetAsync(int id,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, id, nameof(id));

        var result = await CacheManager.GetAsync(CacheKeyDetailTemplate.FormatSmart(id), async () =>
        {
            await using (var scopedContext =
                         await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                return await scopedContext
                    .Albums
                    .Include(x => x.Artist).ThenInclude(x => x.Library)
                    .Include(x => x.Contributors).ThenInclude(x => x.Artist)
                    .Include(x => x.Songs).ThenInclude(x => x.Contributors).ThenInclude(x => x.Artist)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                    .ConfigureAwait(false);
            }
        }, cancellationToken);
        return new MelodeeModels.OperationResult<Album?>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<Album?>> GetByMusicBrainzIdAsync(Guid musicBrainzId,
        CancellationToken cancellationToken = default)
    {
        var id = await CacheManager.GetAsync(
            CacheKeyDetailByMusicBrainzIdTemplate.FormatSmart(musicBrainzId.ToString()), async () =>
            {
                await using (var scopedContext =
                             await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
                {
                    var dbConn = scopedContext.Database.GetDbConnection();
                    return await dbConn
                        .QuerySingleOrDefaultAsync<int?>(
                            "SELECT \"Id\" FROM \"Albums\" WHERE \"MusicBrainzId\" = @musicBrainzId",
                            new { musicBrainzId })
                        .ConfigureAwait(false);
                }
            }, cancellationToken);
        if (id == null)
        {
            return new MelodeeModels.OperationResult<Album?>("Unknown album.")
            {
                Data = null
            };
        }

        return await GetAsync(id.Value, cancellationToken).ConfigureAwait(false);
    }

    public async Task<MelodeeModels.OperationResult<Album?>> GetByApiKeyAsync(Guid apiKey,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(_ => apiKey == Guid.Empty, apiKey, nameof(apiKey));

        var id = await CacheManager.GetAsync(CacheKeyDetailByApiKeyTemplate.FormatSmart(apiKey), async () =>
        {
            await using (var scopedContext =
                         await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var dbConn = scopedContext.Database.GetDbConnection();
                return await dbConn
                    .QuerySingleOrDefaultAsync<int?>("SELECT \"Id\" FROM \"Albums\" WHERE \"ApiKey\" = @apiKey",
                        new { apiKey })
                    .ConfigureAwait(false);
            }
        }, cancellationToken);
        if (id == null)
        {
            return new MelodeeModels.OperationResult<Album?>("Unknown album.")
            {
                Data = null
            };
        }

        return await GetAsync(id.Value, cancellationToken).ConfigureAwait(false);
    }

    public void ClearCache(Album album)
    {
        OpenSubsonicApiService.ClearImageCacheForApiId(album.ToApiKey(), CacheManager);
        CacheManager.Remove(CacheKeyDetailByApiKeyTemplate.FormatSmart(album.ApiKey));
        CacheManager.Remove(CacheKeyDetailByNameNormalizedTemplate.FormatSmart(album.NameNormalized));
        CacheManager.Remove(CacheKeyDetailTemplate.FormatSmart(album.Id));
        if (album.MusicBrainzId != null)
        {
            CacheManager.Remove(
                CacheKeyDetailByMusicBrainzIdTemplate.FormatSmart(album.MusicBrainzId.Value.ToString()));
        }
    }

    public async Task<MelodeeModels.OperationResult<bool>> UpdateAsync(Album album,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(album, nameof(album));

        var validationResult = ValidateModel(album);
        if (!validationResult.IsSuccess)
        {
            return new MelodeeModels.OperationResult<bool>(validationResult.Data.Item2
                ?.Where(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)).Select(x => x.ErrorMessage!).ToArray() ?? [])
            {
                Data = false,
                Type = MelodeeModels.OperationResponseType.ValidationFailure
            };
        }

        bool result;
        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbDetail = await scopedContext
                .Albums
                .Include(x => x.Artist).ThenInclude(x => x.Library)
                .FirstOrDefaultAsync(x => x.Id == album.Id, cancellationToken)
                .ConfigureAwait(false);

            if (dbDetail == null)
            {
                return new MelodeeModels.OperationResult<bool>
                {
                    Data = false,
                    Type = MelodeeModels.OperationResponseType.NotFound
                };
            }
            
            var didChangeName = dbDetail.Name != album.Name;

            var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken);

            var albumDirectory = album.ToMelodeeAlbumModel().AlbumDirectoryName(configuration.Configuration);
            if (!albumDirectory.ToFileSystemDirectoryInfo().ToDirectoryInfo().IsSameDirectory(dbDetail.Directory))
            {
                // Details that are used to build the albums directory has changed, rename directory to new name
                var existingAlbumDirectory = Path.Combine(dbDetail.Artist.Library.Path, dbDetail.Artist.Directory,
                    dbDetail.Directory);
                var newAlbumDirectory =
                    Path.Combine(dbDetail.Artist.Library.Path, dbDetail.Artist.Directory, albumDirectory);
                if (Directory.Exists(existingAlbumDirectory))
                {
                    Directory.Move(existingAlbumDirectory, newAlbumDirectory);
                }
                album.Directory = albumDirectory;
            }
            dbDetail.Directory = album.Directory;
            
            dbDetail.AlbumStatus = album.AlbumStatus;
            dbDetail.AlbumType = album.AlbumType;
            dbDetail.AlternateNames = album.AlternateNames;
            dbDetail.AmgId = album.AmgId;
            dbDetail.ArtistId = album.ArtistId;
            dbDetail.Comment = album.Comment;
            dbDetail.Description = album.Description;
            dbDetail.DeezerId = album.DeezerId;
            dbDetail.DiscogsId = album.DiscogsId;
            dbDetail.Genres = album.Genres;
            dbDetail.ImageCount = album.ImageCount;
            dbDetail.IsCompilation = album.IsCompilation;
            dbDetail.IsLocked = album.IsLocked;
            dbDetail.ItunesId = album.ItunesId;
            dbDetail.LastFmId = album.LastFmId;
            dbDetail.LastUpdatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow);
            dbDetail.Moods = album.Moods;
            dbDetail.MusicBrainzId = album.MusicBrainzId;
            dbDetail.Name = album.Name;
            dbDetail.NameNormalized = album.Name.ToNormalizedString() ?? album.Name;
            dbDetail.Notes = album.Notes;
            dbDetail.OriginalReleaseDate = album.OriginalReleaseDate;
            dbDetail.ReleaseDate = album.ReleaseDate;
            dbDetail.SortName = album.SortName;
            dbDetail.SortOrder = album.SortOrder;
            dbDetail.SpotifyId = album.SpotifyId;
            dbDetail.Tags = album.Tags;
            dbDetail.WikiDataId = album.WikiDataId;

            result = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;

            if (result)
            {
                ClearCache(dbDetail);

                if (didChangeName)
                {
                    await mediaEditService.InitializeAsync(token: cancellationToken);
                    var newAlbumPath = Path.Combine(dbDetail.Artist.Library.Path, dbDetail.Artist.Directory, dbDetail.Directory);
                    var melodeeAlbum = await MelodeeModels.Album.DeserializeAndInitializeAlbumAsync(serializer, Path.Combine(newAlbumPath, "melodee.json"), cancellationToken).ConfigureAwait(false);
                    if (melodeeAlbum != null)
                    {
                        melodeeAlbum.AlbumDbId = dbDetail.Id;
                        melodeeAlbum.Directory = newAlbumPath.ToFileSystemDirectoryInfo();
                        melodeeAlbum.MusicBrainzId = dbDetail.MusicBrainzId;
                        melodeeAlbum.SpotifyId = dbDetail.SpotifyId;
                        melodeeAlbum.SetTagValue(MetaTagIdentifier.Album, dbDetail.Name);
                        foreach(var song in melodeeAlbum.Songs ?? [])
                        {
                            melodeeAlbum.SetSongTagValue(song.Id, MetaTagIdentifier.Album, dbDetail.Name);
                        }
                        await mediaEditService.SaveMelodeeAlbum(melodeeAlbum, true, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
        }


        return new MelodeeModels.OperationResult<bool>
        {
            Data = result
        };
    }


    public async Task<MelodeeModels.OperationResult<Album?>> FindAlbumAsync(int artistId,
        MelodeeModels.Album melodeeAlbum, CancellationToken cancellationToken = default)
    {
        int? id = null;
        var albumTitle = melodeeAlbum.AlbumTitle()?.CleanStringAsIs() ??
                         throw new Exception("Album title is required.");
        var nameNormalized = albumTitle.ToNormalizedString() ?? albumTitle;

        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbConn = scopedContext.Database.GetDbConnection();
            try
            {
                string sql;

                if (melodeeAlbum.AlbumDbId.HasValue)
                {
                    sql = """
                           select a."Id"
                           from "Albums" a 
                           where a."Id" = @id
                          """;
                    id = await dbConn
                        .QuerySingleOrDefaultAsync<int?>(sql, new { id = melodeeAlbum.AlbumDbId })
                        .ConfigureAwait(false);
                }

                if (id == null && melodeeAlbum.Id != Guid.Empty)
                {
                    sql = """
                           select a."Id"
                           from "Albums" a 
                           where a."ApiKey" = @apiKey
                          """;
                    id = await dbConn
                        .QuerySingleOrDefaultAsync<int?>(sql, new { apiKey = melodeeAlbum.Id })
                        .ConfigureAwait(false);
                }

                if (id == null)
                {
                    sql = """
                          select a."Id"
                          from "Albums" a
                          where a."ArtistId" = @artistId
                          and (a."NameNormalized" = @name
                          or a."MusicBrainzId" = @musicBrainzId   
                          or a."SpotifyId" = @spotifyId);
                          """;
                    id = await dbConn
                        .QuerySingleOrDefaultAsync<int?>(sql, new
                        {
                            artistId,
                            name = nameNormalized,
                            musicBrainzId = melodeeAlbum.MusicBrainzId,
                            spotifyId = melodeeAlbum.SpotifyId
                        })
                        .ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e,
                    "[{ServiceName}] attempting to Find Album id [{Id}], apiKey [{ApiKey}], name [{Name}] musicbrainzId [{MbId}] spotifyId [{SpotifyId}].",
                    nameof(ArtistService),
                    melodeeAlbum.AlbumDbId,
                    melodeeAlbum.Id,
                    nameNormalized,
                    melodeeAlbum.MusicBrainzId,
                    melodeeAlbum.SpotifyId);
            }
        }

        if (id == null)
        {
            return new MelodeeModels.OperationResult<Album?>("Unknown album.")
            {
                Data = null
            };
        }

        return await GetAsync(id.Value, cancellationToken).ConfigureAwait(false);
    }

    public async Task<MelodeeModels.OperationResult<bool>> RescanAsync(int[] albumIds,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrEmpty(albumIds, nameof(albumIds));

        var result = false;

        foreach (var albumId in albumIds)
        {
            var albumResult = await GetAsync(albumId, cancellationToken).ConfigureAwait(false);
            if (!albumResult.IsSuccess)
            {
                return new MelodeeModels.OperationResult<bool>("Unknown album.")
                {
                    Data = false
                };
            }

            var album = albumResult.Data!;
            var albumDirectory = Path.Combine(album.Artist.Library.Path, album.Artist.Directory, album.Directory);
            if (!Directory.Exists(albumDirectory))
            {
                Logger.Error("Album directory [{AlbumDirectory}] does not exist.", albumDirectory);
                return new MelodeeModels.OperationResult<bool>($"Album directory not found [{albumDirectory}].")
                {
                    Data = false
                };
            }

            await bus.SendLocal(new AlbumRescanEvent(album.Id, albumDirectory, false)).ConfigureAwait(false);
        }

        return new MelodeeModels.OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<bool>> LockUnlockAlbumAsync(int albumId, bool doLock,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, albumId, nameof(albumId));

        var artistResult = await GetAsync(albumId, cancellationToken).ConfigureAwait(false);
        if (!artistResult.IsSuccess)
        {
            return new MelodeeModels.OperationResult<bool>($"Unknown album to lock [{albumId}].")
            {
                Data = false
            };
        }

        artistResult.Data!.IsLocked = doLock;
        var result = (await UpdateAsync(artistResult.Data, cancellationToken).ConfigureAwait(false)).Data;
        return new MelodeeModels.OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<Album?>> AddAlbumAsync(Album album,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(album, nameof(album));

        var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken);

        album.ApiKey = Guid.NewGuid();
        album.Directory = album.ToMelodeeAlbumModel().AlbumDirectoryName(configuration.Configuration);
        album.CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow);
        album.MetaDataStatus = (int)MetaDataModelStatus.ReadyToProcess;
        album.NameNormalized = album.NameNormalized.Nullify() ?? album.Name.ToNormalizedString() ?? album.Name;

        var validationResult = ValidateModel(album);
        if (!validationResult.IsSuccess)
        {
            return new MelodeeModels.OperationResult<Album?>(validationResult.Data.Item2
                ?.Where(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)).Select(x => x.ErrorMessage!).ToArray() ?? [])
            {
                Data = null,
                Type = MelodeeModels.OperationResponseType.ValidationFailure
            };
        }

        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            scopedContext.Albums.Add(album);
            var result = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            if (result > 0)
            {
                await UpdateLibraryAggregateStatsByIdAsync(album.Artist.LibraryId, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        return await GetAsync(album.Id, cancellationToken);
    }
}
