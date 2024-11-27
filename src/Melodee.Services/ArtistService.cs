using Ardalis.GuardClauses;
using Dapper;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Services.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SmartFormat;
using MelodeeModels = Melodee.Common.Models;

namespace Melodee.Services;

public class ArtistService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    private const string CacheKeyDetailByApiKeyTemplate = "urn:artist:apikey:{0}";
    private const string CacheKeyDetailByMediaUniqueIdTemplate = "urn:artist:mediauniqueid:{0}";
    private const string CacheKeyDetailByNameNormalizedTemplate = "urn:artist:namenormalized:{0}";
    private const string CacheKeyDetailByMusicBrainzIdTemplate = "urn:artist:musicbrainzid:{0}";
    private const string CacheKeyDetailTemplate = "urn:artist:{0}";

    public async Task<MelodeeModels.PagedResult<Artist>> ListAsync(MelodeeModels.PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        int artistCount;
        Artist[] artists = [];
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var orderBy = pagedRequest.OrderByValue();
            var dbConn = scopedContext.Database.GetDbConnection();
            var countSqlParts = pagedRequest.FilterByParts("SELECT COUNT(*) FROM \"Artists\"");
            artistCount = await dbConn
                .QuerySingleAsync<int>(countSqlParts.Item1, countSqlParts.Item2)
                .ConfigureAwait(false);
            if (!pagedRequest.IsTotalCountOnlyRequest)
            {
                var listSqlParts = pagedRequest.FilterByParts("SELECT * FROM \"Artists\"");
                var listSql = $"{listSqlParts.Item1} ORDER BY {orderBy} OFFSET {pagedRequest.SkipValue} ROWS FETCH NEXT {pagedRequest.TakeValue} ROWS ONLY;";
                if (dbConn is SqliteConnection)
                {
                    listSql = $"{listSqlParts.Item1} ORDER BY {orderBy} LIMIT {pagedRequest.TakeValue} OFFSET {pagedRequest.SkipValue};";
                }

                artists = (await dbConn
                    .QueryAsync<Artist>(listSql, listSqlParts.Item2)
                    .ConfigureAwait(false)).ToArray();
            }
        }

        return new MelodeeModels.PagedResult<Artist>
        {
            TotalCount = artistCount,
            TotalPages = pagedRequest.TotalPages(artistCount),
            Data = artists
        };
    }

    public async Task<MelodeeModels.OperationResult<Artist?>> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, id, nameof(id));

        var result = await CacheManager.GetAsync(CacheKeyDetailTemplate.FormatSmart(id), async () =>
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                return await scopedContext
                    .Artists
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                    .ConfigureAwait(false);
            }
        }, cancellationToken);
        return new MelodeeModels.OperationResult<Artist?>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<Artist?>> GetByMusicBrainzIdAsync(Guid musicBrainzId, CancellationToken cancellationToken = default)
    {
        var id = await CacheManager.GetAsync(CacheKeyDetailByMusicBrainzIdTemplate.FormatSmart(musicBrainzId.ToString()), async () =>
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var dbConn = scopedContext.Database.GetDbConnection();
                return await dbConn
                    .QuerySingleOrDefaultAsync<int?>("SELECT \"Id\" FROM \"Artists\" WHERE \"MusicBrainzId\" = @musicBrainzId", new { musicBrainzId })
                    .ConfigureAwait(false);
            }
        }, cancellationToken);
        if (id == null)
        {
            return new MelodeeModels.OperationResult<Artist?>("Unknown artist.")
            {
                Data = null
            };
        }
        return await GetAsync(id.Value, cancellationToken).ConfigureAwait(false);
    }

    public async Task<MelodeeModels.OperationResult<Artist?>> GetByNameNormalized(string nameNormalized, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrEmpty(nameNormalized, nameof(nameNormalized));

        var id = await CacheManager.GetAsync(CacheKeyDetailByNameNormalizedTemplate.FormatSmart(nameNormalized), async () =>
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var dbConn = scopedContext.Database.GetDbConnection();
                return await dbConn
                    .QuerySingleOrDefaultAsync<int?>("SELECT \"Id\" FROM \"Artists\" WHERE \"NameNormalized\" = @nameNormalized", new { nameNormalized })
                    .ConfigureAwait(false);
            }
        }, cancellationToken);
        if (id == null)
        {
            return new MelodeeModels.OperationResult<Artist?>("Unknown artist.")
            {
                Data = null
            };
        }
        return await GetAsync(id.Value, cancellationToken).ConfigureAwait(false);
    }

    public void ClearCache(Artist artist)
    {
        CacheManager.Remove(CacheKeyDetailByMediaUniqueIdTemplate.FormatSmart(artist.MediaUniqueId));
        CacheManager.Remove(CacheKeyDetailByApiKeyTemplate.FormatSmart(artist.ApiKey));
        CacheManager.Remove(CacheKeyDetailByNameNormalizedTemplate.FormatSmart(artist.NameNormalized));
        CacheManager.Remove(CacheKeyDetailTemplate.FormatSmart(artist.Id));
        if (artist.MusicBrainzId != null)
        {
            CacheManager.Remove(CacheKeyDetailByMusicBrainzIdTemplate.FormatSmart(artist.MusicBrainzId.Value.ToString()));
        }        
    }

    public async Task<MelodeeModels.OperationResult<Artist?>> GetByMediaUniqueId(long mediaUniqueId, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, mediaUniqueId, nameof(mediaUniqueId));

        var id = await CacheManager.GetAsync(CacheKeyDetailByMediaUniqueIdTemplate.FormatSmart(mediaUniqueId), async () =>
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var dbConn = scopedContext.Database.GetDbConnection();
                return await dbConn
                    .QuerySingleOrDefaultAsync<int?>("SELECT \"Id\" FROM \"Artists\" WHERE \"MediaUniqueId\" = @mediaUniqueId", new { mediaUniqueId })
                    .ConfigureAwait(false);
            }
        }, cancellationToken);
        if (id == null)
        {
            return new MelodeeModels.OperationResult<Artist?>("Unknown artist.")
            {
                Data = null
            };
        }

        return await GetAsync(id.Value, cancellationToken).ConfigureAwait(false);
    }


    public async Task<MelodeeModels.OperationResult<Artist?>> GetByApiKeyAsync(Guid apiKey, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => apiKey == Guid.Empty, apiKey, nameof(apiKey));

        var id = await CacheManager.GetAsync(CacheKeyDetailByApiKeyTemplate.FormatSmart(apiKey), async () =>
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var dbConn = scopedContext.Database.GetDbConnection();
                return await dbConn
                    .QuerySingleOrDefaultAsync<int?>("SELECT \"Id\" FROM \"Artists\" WHERE \"ApiKey\" = @apiKey", new { apiKey })
                    .ConfigureAwait(false);
            }
        }, cancellationToken);
        if (id == null)
        {
            return new MelodeeModels.OperationResult<Artist?>("Unknown artist.")
            {
                Data = null
            };
        }

        return await GetAsync(id.Value, cancellationToken).ConfigureAwait(false);
    }

    public async Task ClearCacheAsync(int artistId, CancellationToken cancellationToken)
    {
        var artist = await GetAsync(artistId, cancellationToken).ConfigureAwait(false);
        if (artist?.Data != null)
        {
            CacheManager.Remove(CacheKeyDetailByApiKeyTemplate.FormatSmart(artist.Data.ApiKey));
            CacheManager.Remove(CacheKeyDetailByMediaUniqueIdTemplate.FormatSmart(artist.Data.MediaUniqueId));
            CacheManager.Remove(CacheKeyDetailByNameNormalizedTemplate.FormatSmart(artist.Data.MediaUniqueId));
            CacheManager.Remove(CacheKeyDetailTemplate.FormatSmart(artist.Data.Id));
        }
    }
}
