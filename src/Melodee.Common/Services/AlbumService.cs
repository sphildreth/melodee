using Ardalis.GuardClauses;
using Dapper;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Models.Collection;
using Melodee.Common.Services.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SmartFormat;
using MelodeeModels = Melodee.Common.Models;

namespace Melodee.Common.Services;

public class AlbumService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    private const string CacheKeyDetailByApiKeyTemplate = "urn:album:apikey:{0}";
    private const string CacheKeyDetailByNameNormalizedTemplate = "urn:album:namenormalized:{0}";
    private const string CacheKeyDetailByMusicBrainzIdTemplate = "urn:album:musicbrainzid:{0}";
    private const string CacheKeyDetailTemplate = "urn:album:{0}";

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
                CacheManager.Remove(CacheKeyDetailByMusicBrainzIdTemplate.FormatSmart(album.Data.MusicBrainzId.Value.ToString()));
            }
        }
    }

    public async Task<MelodeeModels.PagedResult<AlbumDataInfo>> ListForArtistApiKeyAsync(MelodeeModels.PagedRequest pagedRequest, Guid filterToArtistApiKey, CancellationToken cancellationToken = default)
    {
        int albumCount;
        AlbumDataInfo[] albums = [];
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            albumCount = await scopedContext
                .Albums
                .CountAsync(x => x.Artist.ApiKey == filterToArtistApiKey, cancellationToken)
                .ConfigureAwait(false);
            if (!pagedRequest.IsTotalCountOnlyRequest)
            {
                var aristAlbums = await scopedContext
                    .Albums.Include(x => x.Artist)
                    .Where(x => x.Artist.ApiKey == filterToArtistApiKey)
                    .OrderByDescending(x => x.ReleaseDate).ThenBy(x => x.Name)
                    .Skip(pagedRequest.SkipValue)
                    .Take(pagedRequest.TakeValue)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
                albums = aristAlbums.Select(x => x.ToAlbumDataInfo()).ToArray();
            }
        }

        return new MelodeeModels.PagedResult<AlbumDataInfo>
        {
            TotalCount = albumCount,
            TotalPages = pagedRequest.TotalPages(albumCount),
            Data = albums
        };

    }

    public async Task<MelodeeModels.PagedResult<AlbumDataInfo>> ListAsync(MelodeeModels.PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        int albumCount;
        AlbumDataInfo[] albums = [];
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var orderBy = pagedRequest.OrderByValue();
            var dbConn = scopedContext.Database.GetDbConnection();
            var countSqlParts = pagedRequest.FilterByParts("SELECT COUNT(*) FROM \"Albums\"");
            albumCount = await dbConn
                .QuerySingleAsync<int>(countSqlParts.Item1, countSqlParts.Item2)
                .ConfigureAwait(false);
            if (!pagedRequest.IsTotalCountOnlyRequest)
            {
                var sqlStartFragment = """
                                       SELECT a."Id", a."ApiKey", a."IsLocked", a."Name", a."NameNormalized", a."AlternateNames",
                                              ar."ApiKey" as "ArtistApiKey", ar."Name" as "ArtistName",
                                              a."DiscCount", a."SongCount", a."Duration", a."CreatedAt", a."Tags"
                                       FROM "Albums" a
                                       JOIN "Artists" ar ON (a."ArtistId" = ar."Id")
                                       """;                
                var listSqlParts = pagedRequest.FilterByParts(sqlStartFragment);
                var listSql = $"{listSqlParts.Item1} ORDER BY {orderBy} OFFSET {pagedRequest.SkipValue} ROWS FETCH NEXT {pagedRequest.TakeValue} ROWS ONLY;";
                if (dbConn is SqliteConnection)
                {
                    listSql = $"{listSqlParts.Item1} ORDER BY {orderBy} LIMIT {pagedRequest.TakeValue} OFFSET {pagedRequest.SkipValue};";
                }

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

    public async Task<MelodeeModels.OperationResult<bool>> DeleteAsync(int[] albumIds, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrEmpty(albumIds, nameof(albumIds));

        bool result;

        var artistIds = new List<int>();
        var libraryIds = new List<int>();

        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
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

            result = true;
        }

        return new MelodeeModels.OperationResult<bool>
        {
            Data = result
        };
    }    
    
    public async Task<MelodeeModels.OperationResult<Album?>> GetByArtistIdAndNameNormalized(int artistId, string nameNormalized, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrEmpty(nameNormalized, nameof(nameNormalized));

        int? id = null;
        try
        {
            id = await CacheManager.GetAsync(CacheKeyDetailByNameNormalizedTemplate.FormatSmart(nameNormalized), async () =>
            {
                await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
                {
                    var dbConn = scopedContext.Database.GetDbConnection();
                    return await dbConn
                        .QuerySingleOrDefaultAsync<int?>("SELECT \"Id\" FROM \"Albums\" WHERE \"ArtistId\" = @artistId AND \"NameNormalized\" = @nameNormalized", new { artistId, nameNormalized })
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

    public async Task<MelodeeModels.OperationResult<Album?>> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, id, nameof(id));

        var result = await CacheManager.GetAsync(CacheKeyDetailTemplate.FormatSmart(id), async () =>
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                return await scopedContext
                    .Albums
                    .Include(x => x.Artist)
                    .Include(x => x.Contributors).ThenInclude(x => x.Artist)
                    .Include(x => x.Discs).ThenInclude(x => x.Songs).ThenInclude(x => x.Contributors).ThenInclude(x => x.Artist)
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

    public async Task<MelodeeModels.OperationResult<Album?>> GetByMusicBrainzIdAsync(Guid musicBrainzId, CancellationToken cancellationToken = default)
    {
        var id = await CacheManager.GetAsync(CacheKeyDetailByMusicBrainzIdTemplate.FormatSmart(musicBrainzId.ToString()), async () =>
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var dbConn = scopedContext.Database.GetDbConnection();
                return await dbConn
                    .QuerySingleOrDefaultAsync<int?>("SELECT \"Id\" FROM \"Albums\" WHERE \"MusicBrainzId\" = @musicBrainzId", new { musicBrainzId })
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

    public async Task<MelodeeModels.OperationResult<Album?>> GetByApiKeyAsync(Guid apiKey, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(_ => apiKey == Guid.Empty, apiKey, nameof(apiKey));

        var id = await CacheManager.GetAsync(CacheKeyDetailByApiKeyTemplate.FormatSmart(apiKey), async () =>
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var dbConn = scopedContext.Database.GetDbConnection();
                return await dbConn
                    .QuerySingleOrDefaultAsync<int?>("SELECT \"Id\" FROM \"Albums\" WHERE \"ApiKey\" = @apiKey", new { apiKey })
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
}
