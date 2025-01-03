using Ardalis.GuardClauses;
using Dapper;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Services.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;
using SmartFormat;
using MelodeeModels = Melodee.Common.Models;

namespace Melodee.Common.Services;

public class ArtistService(
    ILogger logger,
    ICacheManager cacheManager,
    ISettingService settingService,
    IDbContextFactory<MelodeeDbContext> contextFactory)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    private const string CacheKeyDetailByApiKeyTemplate = "urn:artist:apikey:{0}";
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
                    .Include(x => x.Library)
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
        CacheManager.Remove(CacheKeyDetailByApiKeyTemplate.FormatSmart(artist.ApiKey));
        CacheManager.Remove(CacheKeyDetailByNameNormalizedTemplate.FormatSmart(artist.NameNormalized));
        CacheManager.Remove(CacheKeyDetailTemplate.FormatSmart(artist.Id));
        if (artist.MusicBrainzId != null)
        {
            CacheManager.Remove(CacheKeyDetailByMusicBrainzIdTemplate.FormatSmart(artist.MusicBrainzId.Value.ToString()));
        }
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
            CacheManager.Remove(CacheKeyDetailByNameNormalizedTemplate.FormatSmart(artist.Data.NameNormalized));
            CacheManager.Remove(CacheKeyDetailTemplate.FormatSmart(artist.Data.Id));
        }
    }

    public async Task<MelodeeModels.OperationResult<bool>> DeleteAsync(int[] artistIds, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrEmpty(artistIds, nameof(artistIds));

        bool result;

        var libraryIds = new List<int>();

        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            foreach (var artistId in artistIds)
            {
                var artist = await GetAsync(artistId, cancellationToken).ConfigureAwait(false);
                if (!artist.IsSuccess)
                {
                    return new MelodeeModels.OperationResult<bool>("Unknown artist.")
                    {
                        Data = false
                    };
                }
            }

            foreach (var artistId in artistIds)
            {
                var artist = await scopedContext
                    .Artists.Include(x => x.Library)
                    .FirstAsync(x => x.Id == artistId, cancellationToken)
                    .ConfigureAwait(false);

                var artistDirectory = Path.Combine(artist.Library.Path, artist.Directory);
                if (Directory.Exists(artistDirectory))
                {
                    Directory.Delete(artistDirectory, true);
                }

                scopedContext.Artists.Remove(artist);
                libraryIds.Add(artist.LibraryId);
            }

            await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
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

    public async Task<MelodeeModels.OperationResult<bool>> UpdateAsync(Artist artist, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(artist, nameof(artist));

        var validationResult = ValidateModel(artist);
        if (!validationResult.IsSuccess)
        {
            return new MelodeeModels.OperationResult<bool>(validationResult.Data.Item2?.Where(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)).Select(x => x.ErrorMessage!).ToArray() ?? [])
            {
                Data = false,
                Type = MelodeeModels.OperationResponseType.ValidationFailure
            };
        }

        bool result;
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbDetail = await scopedContext
                .Artists
                .FirstOrDefaultAsync(x => x.Id == artist.Id, cancellationToken)
                .ConfigureAwait(false);

            if (dbDetail == null)
            {
                return new MelodeeModels.OperationResult<bool>
                {
                    Data = false,
                    Type = MelodeeModels.OperationResponseType.NotFound
                };
            }

            dbDetail.AlternateNames = artist.AlternateNames;
            dbDetail.AmgId= artist.AmgId; 
            dbDetail.Biography = artist.Biography;
            dbDetail.Description = artist.Description;
            dbDetail.Directory = artist.Directory;
            dbDetail.DiscogsId= artist.DiscogsId;
            dbDetail.ImageCount = artist.ImageCount; 
            dbDetail.IsLocked = artist.IsLocked;
            dbDetail.ItunesId= artist.ItunesId;
            dbDetail.LastFmId= artist.LastFmId;
            dbDetail.LastUpdatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow);
            dbDetail.LibraryId = artist.LibraryId;
            dbDetail.MusicBrainzId= artist.MusicBrainzId;
            dbDetail.Name = artist.Name;
            dbDetail.NameNormalized = artist.NameNormalized;
            dbDetail.Notes = artist.Notes;
            dbDetail.RealName = artist.RealName;
            dbDetail.Roles = artist.Roles;
            dbDetail.SortName = artist.SortName;
            dbDetail.SortOrder = artist.SortOrder;
            dbDetail.SpotifyId= artist.SpotifyId;
            dbDetail.Tags = artist.Tags;
            dbDetail.WikiDataId= artist.WikiDataId;

            result = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;

            if (result)
            {
                ClearCache(dbDetail);
            }
        }


        return new MelodeeModels.OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<Artist?>> AddArtistAsync(Artist artist, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(artist, nameof(artist));

        var configuration = await settingService.GetMelodeeConfigurationAsync(cancellationToken);
        
        artist.ApiKey = Guid.NewGuid();
        artist.Directory = artist.ToMelodeeArtistModel().ToDirectoryName(configuration.GetValue<int>(SettingRegistry.ProcessingMaximumArtistDirectoryNameLength));
        artist.CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow);
        artist.MetaDataStatus = (int)MetaDataModelStatus.ReadyToProcess;
        artist.NameNormalized = artist.NameNormalized.Nullify() ?? artist.Name.ToNormalizedString() ?? artist.Name;
        
        var validationResult = ValidateModel(artist);
        if (!validationResult.IsSuccess)
        {
            return new MelodeeModels.OperationResult<Artist?>(validationResult.Data.Item2?.Where(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)).Select(x => x.ErrorMessage!).ToArray() ?? [])
            {
                Data = null,
                Type = MelodeeModels.OperationResponseType.ValidationFailure
            };
        }

        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            scopedContext.Artists.Add(artist);
            var result = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            if (result > 0)
            {
                await UpdateLibraryAggregateStatsByIdAsync(artist.LibraryId, cancellationToken).ConfigureAwait(false);
            }
        }
        return await GetAsync(artist.Id, cancellationToken);
    }
}
