using Ardalis.GuardClauses;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Filtering;
using Melodee.Common.MessageBus.Events;
using Melodee.Common.Models.Collection;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Plugins.Conversion.Image;
using Melodee.Common.Serialization;
using Melodee.Common.Services.Caching;
using Melodee.Common.Services.Extensions;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Rebus.Bus;
using Serilog;
using SmartFormat;
using MelodeeModels = Melodee.Common.Models;

namespace Melodee.Common.Services;

public class ArtistService(
    ILogger logger,
    ICacheManager cacheManager,
    IMelodeeConfigurationFactory configurationFactory,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    ISerializer serializer,
    IHttpClientFactory httpClientFactory,
    AlbumService albumService,
    IBus bus,
    IFileSystemService fileSystemService)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    private const string CacheKeyDetailByApiKeyTemplate = "urn:artist:apikey:{0}";
    private const string CacheKeyDetailByNameNormalizedTemplate = "urn:artist:namenormalized:{0}";
    private const string CacheKeyDetailByMusicBrainzIdTemplate = "urn:artist:musicbrainzid:{0}";
    private const string CacheKeyDetailTemplate = "urn:artist:{0}";
    private const string CacheKeyArtistImageBytesAndEtagTemplate = "urn:artist:imagebytesandetag:{0}:{1}";

    public async Task<MelodeeModels.PagedResult<ArtistDataInfo>> ListAsync(MelodeeModels.PagedRequest pagedRequest,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        
        // Build the base query with performance optimizations
        var baseQuery = scopedContext.Artists
            .AsNoTracking()
            .Include(a => a.Library);

        // Apply filters using EF Core instead of raw SQL
        var filteredQuery = ApplyFilters(baseQuery, pagedRequest);

        // Get count efficiently
        var artistCount = await filteredQuery.CountAsync(cancellationToken).ConfigureAwait(false);

        ArtistDataInfo[] artists = [];
        if (!pagedRequest.IsTotalCountOnlyRequest)
        {
            // Apply ordering, skip, and take with projection to ArtistDataInfo
            var orderedQuery = ApplyOrdering(filteredQuery, pagedRequest);
            
            artists = await orderedQuery
                .Skip(pagedRequest.SkipValue)
                .Take(pagedRequest.TakeValue)
                .Select(a => new ArtistDataInfo(
                    a.Id,
                    a.ApiKey,
                    a.IsLocked,
                    a.LibraryId,
                    a.Library.Path, // LibraryPath
                    a.Name,
                    a.NameNormalized,
                    a.AlternateNames ?? string.Empty,
                    a.Directory,
                    a.AlbumCount,
                    a.SongCount,
                    a.CreatedAt,
                    a.Tags ?? string.Empty,
                    a.LastUpdatedAt))
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        return new MelodeeModels.PagedResult<ArtistDataInfo>
        {
            TotalCount = artistCount,
            TotalPages = pagedRequest.TotalPages(artistCount),
            Data = artists
        };
    }

    private static IQueryable<Artist> ApplyFilters(IQueryable<Artist> query, MelodeeModels.PagedRequest pagedRequest)
    {
        if (pagedRequest.FilterBy == null || pagedRequest.FilterBy.Length == 0)
        {
            return query;
        }

        foreach (var filter in pagedRequest.FilterBy)
        {
            var filterValue = filter.Value.ToString() ?? string.Empty;
            
            query = filter.PropertyName.ToLowerInvariant() switch
            {
                "name" => filter.Operator switch
                {
                    FilterOperator.Contains => query.Where(a => a.Name.Contains(filterValue)),
                    FilterOperator.Equals => query.Where(a => a.Name == filterValue),
                    FilterOperator.StartsWith => query.Where(a => a.Name.StartsWith(filterValue)),
                    _ => query
                },
                "namenormalized" => filter.Operator switch
                {
                    FilterOperator.Contains => query.Where(a => a.NameNormalized.Contains(filterValue)),
                    FilterOperator.Equals => query.Where(a => a.NameNormalized == filterValue),
                    FilterOperator.StartsWith => query.Where(a => a.NameNormalized.StartsWith(filterValue)),
                    _ => query
                },
                "alternatenames" => filter.Operator switch
                {
                    FilterOperator.Contains => query.Where(a => a.AlternateNames != null && a.AlternateNames.Contains(filterValue)),
                    _ => query
                },
                "islocked" => filter.Operator switch
                {
                    FilterOperator.Equals when bool.TryParse(filterValue, out var boolValue) => 
                        query.Where(a => a.IsLocked == boolValue),
                    _ => query
                },
                _ => query
            };
        }

        return query;
    }

    private static IQueryable<Artist> ApplyOrdering(IQueryable<Artist> query, MelodeeModels.PagedRequest pagedRequest)
    {
        // Use the existing OrderByValue method from PagedRequest
        var orderByClause = pagedRequest.OrderByValue("Name", MelodeeModels.PagedRequest.OrderAscDirection);
        
        // Parse the order by clause to determine field and direction
        var isDescending = orderByClause.Contains("DESC", StringComparison.OrdinalIgnoreCase);
        var fieldName = orderByClause.Split(' ')[0].Trim('"').ToLowerInvariant();

        return fieldName switch
        {
            "name" => isDescending ? query.OrderByDescending(a => a.Name) : query.OrderBy(a => a.Name),
            "namenormalized" => isDescending ? query.OrderByDescending(a => a.NameNormalized) : query.OrderBy(a => a.NameNormalized),
            "createdat" => isDescending ? query.OrderByDescending(a => a.CreatedAt) : query.OrderBy(a => a.CreatedAt),
            "lastupdatedat" => isDescending ? query.OrderByDescending(a => a.LastUpdatedAt) : query.OrderBy(a => a.LastUpdatedAt),
            "albumcount" => isDescending ? query.OrderByDescending(a => a.AlbumCount) : query.OrderBy(a => a.AlbumCount),
            "songcount" => isDescending ? query.OrderByDescending(a => a.SongCount) : query.OrderBy(a => a.SongCount),
            _ => query.OrderBy(a => a.Name)
        };
    }

    public async Task<MelodeeModels.OperationResult<Artist?>> GetAsync(int id,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, id, nameof(id));

        var result = await CacheManager.GetAsync(CacheKeyDetailTemplate.FormatSmart(id), async () =>
        {
            await using (var scopedContext =
                         await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
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

    public async Task<MelodeeModels.OperationResult<Artist?>> GetByMusicBrainzIdAsync(Guid musicBrainzId,
        CancellationToken cancellationToken = default)
    {
        var id = await CacheManager.GetAsync(
            CacheKeyDetailByMusicBrainzIdTemplate.FormatSmart(musicBrainzId.ToString()), async () =>
            {
                await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
                
                return await scopedContext.Artists
                    .AsNoTracking()
                    .Where(a => a.MusicBrainzId == musicBrainzId)
                    .Select(a => (int?)a.Id)
                    .FirstOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);
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

    public async Task<MelodeeModels.OperationResult<Artist?>> GetByNameNormalized(string nameNormalized,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrEmpty(nameNormalized, nameof(nameNormalized));

        var id = await CacheManager.GetAsync(CacheKeyDetailByNameNormalizedTemplate.FormatSmart(nameNormalized),
            async () =>
            {
                await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
                
                return await scopedContext.Artists
                    .AsNoTracking()
                    .Where(a => a.NameNormalized == nameNormalized)
                    .Select(a => (int?)a.Id)
                    .FirstOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);
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


    /// <summary>
    ///     Find the Artist using various given Ids.
    /// </summary>
    public async Task<MelodeeModels.OperationResult<Artist?>> FindArtistAsync(int? byId, Guid byApiKey, string? byName,
        Guid? byMusicBrainzId, string? bySpotifyId, CancellationToken cancellationToken = default)
    {
        int? id = null;

        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        
        try
        {
            // Try to find by ID first (most efficient)
            if (byId.HasValue)
            {
                id = await scopedContext.Artists
                    .AsNoTracking()
                    .Where(a => a.Id == byId.Value)
                    .Select(a => (int?)a.Id)
                    .FirstOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);
            }

            // Try to find by API key
            if (id == null && byApiKey != Guid.Empty)
            {
                id = await scopedContext.Artists
                    .AsNoTracking()
                    .Where(a => a.ApiKey == byApiKey)
                    .Select(a => (int?)a.Id)
                    .FirstOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);
            }

            // Try to find by MusicBrainz ID or Spotify ID
            if (id == null && (byMusicBrainzId != null || bySpotifyId != null))
            {
                id = await scopedContext.Artists
                    .AsNoTracking()
                    .Where(a => (byMusicBrainzId != null && a.MusicBrainzId == byMusicBrainzId) ||
                               (bySpotifyId != null && a.SpotifyId == bySpotifyId))
                    .Select(a => (int?)a.Id)
                    .FirstOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);
            }

            // Finally try to find by normalized name
            if (id == null && !string.IsNullOrEmpty(byName))
            {
                id = await scopedContext.Artists
                    .AsNoTracking()
                    .Where(a => a.NameNormalized == byName)
                    .Select(a => (int?)a.Id)
                    .FirstOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            Logger.Error(e,
                "[{ServiceName}] attempting to Find Artist id [{Id}], apiKey [{ApiKey}], name [{Name}] musicbrainzId [{MbId}] spotifyId [{SpotifyId}]",
                nameof(ArtistService),
                byId,
                byApiKey,
                byName,
                byMusicBrainzId,
                bySpotifyId);
        }

        if (id == null)
        {
            return new MelodeeModels.OperationResult<Artist?>("Unknown artist.")
            {
                Data = null
            };
        }

        return await GetAsync(id.Value, cancellationToken).ConfigureAwait(false);
    }

    public async Task<MelodeeModels.OperationResult<Artist?>> GetByApiKeyAsync(Guid apiKey,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x == Guid.Empty, apiKey, nameof(apiKey));

        var id = await CacheManager.GetAsync(CacheKeyDetailByApiKeyTemplate.FormatSmart(apiKey), async () =>
        {
            await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
            
            return await scopedContext.Artists
                .AsNoTracking()
                .Where(a => a.ApiKey == apiKey)
                .Select(a => (int?)a.Id)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
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

    public async Task ClearCacheAsync(Artist artist, CancellationToken cancellationToken)
    {
        CacheManager.Remove(CacheKeyDetailByApiKeyTemplate.FormatSmart(artist.ApiKey), Artist.CacheRegion);
        CacheManager.Remove(CacheKeyDetailByNameNormalizedTemplate.FormatSmart(artist.NameNormalized), Artist.CacheRegion);
        CacheManager.Remove(CacheKeyDetailTemplate.FormatSmart(artist.Id), Artist.CacheRegion);
        if (artist.MusicBrainzId != null)
        {
            CacheManager.Remove(CacheKeyDetailByMusicBrainzIdTemplate.FormatSmart(artist.MusicBrainzId.Value.ToString()), Artist.CacheRegion);
        }
        
        CacheManager.Remove(CacheKeyArtistImageBytesAndEtagTemplate.FormatSmart(artist.Id, ImageSizeRegistry.Small), Artist.CacheRegion);
        CacheManager.Remove(CacheKeyArtistImageBytesAndEtagTemplate.FormatSmart(artist.Id, ImageSizeRegistry.Medium), Artist.CacheRegion);
        CacheManager.Remove(CacheKeyArtistImageBytesAndEtagTemplate.FormatSmart(artist.Id, ImageSizeRegistry.Large), Artist.CacheRegion);
        
        // This is needed because the OpenSubsonicApiService caches the image bytes after potentially resizing
        CacheManager.ClearRegion(OpenSubsonicApiService.ImageCacheRegion);        
        
        await albumService.ClearCacheForArtist(artist.Id, cancellationToken);
    }

    public async Task ClearCacheAsync(int artistId, CancellationToken cancellationToken)
    {
        var artist = await GetAsync(artistId, cancellationToken).ConfigureAwait(false);
        await ClearCacheAsync(artist.Data!, cancellationToken);
    }

    public async Task<MelodeeModels.OperationResult<bool>> RescanAsync(int[] artistIds, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrEmpty(artistIds, nameof(artistIds));

        foreach (var artistId in artistIds)
        {
            var artistResult = await GetAsync(artistId, cancellationToken).ConfigureAwait(false);
            if (!artistResult.IsSuccess || artistResult.Data == null)
            {
                return new MelodeeModels.OperationResult<bool>("Unknown artist.")
                {
                    Data = false
                };
            }

            await bus.SendLocal(new ArtistRescanEvent(artistResult.Data.Id,
                    Path.Combine(artistResult.Data.Library.Path,
                        artistResult.Data.Directory)))
                .ConfigureAwait(false);
        }

        return new MelodeeModels.OperationResult<bool>
        {
            Data = true
        };
    }

    public async Task<MelodeeModels.OperationResult<bool>> DeleteAsync(int[] artistIds,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrEmpty(artistIds, nameof(artistIds));

        bool result;

        var libraryIds = new List<int>();

        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
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
                if (fileSystemService.DirectoryExists(artistDirectory))
                {
                    fileSystemService.DeleteDirectory(artistDirectory, true);
                }

                var artistContributors = await scopedContext.Contributors.Where(x => x.ArtistId == artistId)
                    .ToListAsync(cancellationToken).ConfigureAwait(false);
                if (artistContributors.Count > 0)
                {
                    foreach (var artistContributor in artistContributors)
                    {
                        scopedContext.Contributors.Remove(artistContributor);
                    }
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

    public async Task<MelodeeModels.OperationResult<bool>> UpdateAsync(Artist artist,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(artist, nameof(artist));

        var validationResult = ValidateModel(artist);
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
                .Artists
                .Include(x => x.Library)
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

            dbDetail.Directory = artist.Directory;

            var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken);

            var newArtistDirectory = artist.ToMelodeeArtistModel().ToDirectoryName(configuration.GetValue<int>(SettingRegistry.ProcessingMaximumArtistDirectoryNameLength));
            var newDirectory = Path.Combine(dbDetail.Library.Path, newArtistDirectory);
            var originalDirectoryPath = Path.Combine(dbDetail.Library.Path, dbDetail.Directory);
            
            // Check if we need to move the directory
            if (originalDirectoryPath != newDirectory)
            {
                fileSystemService.MoveDirectory(originalDirectoryPath, newDirectory);
                dbDetail.Directory = newArtistDirectory;
            }

            dbDetail.AlternateNames = artist.AlternateNames;
            dbDetail.AmgId = artist.AmgId;
            dbDetail.Biography = artist.Biography.Nullify();
            dbDetail.Description = artist.Description;

            dbDetail.DeezerId = artist.DeezerId;
            dbDetail.DiscogsId = artist.DiscogsId;
            dbDetail.ImageCount = artist.ImageCount;
            dbDetail.IsLocked = artist.IsLocked;
            dbDetail.ItunesId = artist.ItunesId;
            dbDetail.LastFmId = artist.LastFmId;
            dbDetail.LastUpdatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow);
            dbDetail.LibraryId = artist.LibraryId;
            dbDetail.MusicBrainzId = artist.MusicBrainzId;
            dbDetail.Name = artist.Name;
            dbDetail.NameNormalized = artist.NameNormalized;
            dbDetail.Notes = artist.Notes;
            dbDetail.RealName = artist.RealName;
            dbDetail.Roles = artist.Roles;
            dbDetail.SortName = artist.SortName;
            dbDetail.SortOrder = artist.SortOrder;
            dbDetail.SpotifyId = artist.SpotifyId;
            dbDetail.Tags = artist.Tags;
            dbDetail.WikiDataId = artist.WikiDataId;

            result = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;

            if (result)
            {
                await ClearCacheAsync(dbDetail, cancellationToken);
            }
        }


        return new MelodeeModels.OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<Artist?>> AddArtistAsync(Artist artist,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(artist, nameof(artist));

        var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken);

        artist.ApiKey = Guid.NewGuid();
        artist.Directory = artist.ToMelodeeArtistModel()
            .ToDirectoryName(configuration.GetValue<int>(SettingRegistry.ProcessingMaximumArtistDirectoryNameLength));
        artist.CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow);
        artist.MetaDataStatus = (int)MetaDataModelStatus.ReadyToProcess;
        artist.NameNormalized = artist.NameNormalized.Nullify() ?? artist.Name.ToNormalizedString() ?? artist.Name;

        var validationResult = ValidateModel(artist);
        if (!validationResult.IsSuccess)
        {
            return new MelodeeModels.OperationResult<Artist?>(validationResult.Data.Item2
                ?.Where(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)).Select(x => x.ErrorMessage!).ToArray() ?? [])
            {
                Data = null,
                Type = MelodeeModels.OperationResponseType.ValidationFailure
            };
        }

        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
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

    public async Task<MelodeeModels.OperationResult<bool>> SaveImageAsArtistImageAsync(
        int artistId,
        bool deleteAllImages, 
        byte[] imageBytes, 
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, artistId, nameof(artistId));
        Guard.Against.NullOrEmpty(imageBytes, nameof(imageBytes));

        var artist = await GetAsync(artistId, cancellationToken);
        if (!artist.IsSuccess || artist.Data == null)
        {
            return new MelodeeModels.OperationResult<bool>("Unknown artist.")
            {
                Data = false
            };
        }

        return new MelodeeModels.OperationResult<bool>
        {
            Data = await SaveImageBytesAsArtistImageAsync(
                    artist.Data, 
                    deleteAllImages, 
                    imageBytes, 
                    cancellationToken)
                .ConfigureAwait(false)
        };
    }

    private async Task<bool> SaveImageBytesAsArtistImageAsync(
        Artist artist, 
        bool deleteAllImages, 
        byte[] imageBytes,
        CancellationToken cancellationToken = default)
    {
        var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken);
        var imageConvertor = new ImageConvertor(configuration);
        var artistDirectory = artist.ToFileSystemDirectoryInfo();
        var artistImages = artistDirectory.FileInfosForExtension("jpg", false).ToArray();
        if (deleteAllImages && artistImages.Length != 0)
        {
            foreach (var fileInAlbumDirectory in artistImages)
            {
                fileInAlbumDirectory.Delete();
            }
            artistImages = artistDirectory.FileInfosForExtension("jpg", false).ToArray();
        }
        var totalArtistImageCount = artistImages.Length == 1 ? 1 : artistImages.Length + 1;
        var artistImageFileName = Path.Combine(artistDirectory.Path, deleteAllImages ? "01-Band.image" : $"{totalArtistImageCount}-Band.image");
        var artistImageFileInfo = new FileInfo(artistImageFileName).ToFileSystemInfo();

        await fileSystemService.WriteAllBytesAsync(artistImageFileInfo.FullName(artistDirectory), imageBytes, cancellationToken);
        await imageConvertor.ProcessFileAsync(
            artistDirectory,
            artistImageFileInfo,
            cancellationToken);
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
            await scopedContext.Artists
                .Where(x => x.Id == artist.Id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.LastUpdatedAt, now)
                    .SetProperty(x => x.ImageCount, totalArtistImageCount), cancellationToken)
                .ConfigureAwait(false);
        }
        await ClearCacheAsync(artist, cancellationToken);
        Logger.Information("Saved image for artist [{ArtistId}] with {ImageCount} images.",
            artist.Id, totalArtistImageCount);
        return true;
    }

    public async Task<MelodeeModels.OperationResult<bool>> SaveImageUrlAsArtistImageAsync(
        int artistId, 
        string imageUrl,
        bool deleteAllImages, 
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, artistId, nameof(artistId));
        Guard.Against.NullOrEmpty(imageUrl, nameof(imageUrl));

        var artist = await GetAsync(artistId, cancellationToken);
        if (!artist.IsSuccess || artist.Data == null)
        {
            return new MelodeeModels.OperationResult<bool>("Unknown artist.")
            {
                Data = false
            };
        }

        var result = false;
        var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken);
        try
        {
            var imageBytes = await httpClientFactory.BytesForImageUrlAsync(
                configuration.GetValue<string?>(SettingRegistry.SearchEngineUserAgent) ?? string.Empty, 
                imageUrl,
                cancellationToken);
            if (imageBytes != null)
            {
                result = await SaveImageBytesAsArtistImageAsync(
                    artist.Data, 
                    deleteAllImages, 
                    imageBytes,
                    cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "Error attempting to download mage Url [{Url}] for artist [{Artist}]", imageUrl,
                artist.Data.ToString());
        }

        return new MelodeeModels.OperationResult<bool>("An error has occured. OH NOES!")
        {
            Data = result
        };
    }


    /// <summary>
    ///     Merge all artists to merge into the merge into artist
    /// </summary>
    /// <param name="artistIdToMergeInfo">The artist to merge the other artists into.</param>
    /// <param name="artistIdsToMerge">Artists to merge.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task<MelodeeModels.OperationResult<bool>> MergeArtistsAsync(int artistIdToMergeInfo,
        int[] artistIdsToMerge, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, artistIdToMergeInfo, nameof(artistIdToMergeInfo));
        Guard.Against.NullOrEmpty(artistIdsToMerge, nameof(artistIdsToMerge));

        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken);

            var dbArtistToMergeInto = await scopedContext
                .Artists
                .Include(x => x.Library)
                .FirstOrDefaultAsync(x => x.Id == artistIdToMergeInfo, cancellationToken)
                .ConfigureAwait(false);

            if (dbArtistToMergeInto == null)
            {
                return new MelodeeModels.OperationResult<bool>($"Unknown artist to merge into [{artistIdToMergeInfo}].")
                {
                    Data = false
                };
            }

            var dbArtistToMergeIntoDirectoryPath = dbArtistToMergeInto.ToFileSystemDirectoryInfo().FullName();
            if (!fileSystemService.DirectoryExists(dbArtistToMergeIntoDirectoryPath))
            {
                fileSystemService.CreateDirectory(dbArtistToMergeIntoDirectoryPath);
            }

            var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
            var libraryIdsToUpdate = new List<int>();
            var artistAlternateNamesToMerge = new List<string>();
            foreach (var artistApiKeyToMerge in artistIdsToMerge)
            {
                var dbArtist = await scopedContext
                    .Artists
                    .Include(x => x.Library)
                    .Include(x => x.Albums)
                    .Include(x => x.UserArtists)
                    .FirstOrDefaultAsync(x => x.Id == artistApiKeyToMerge, cancellationToken)
                    .ConfigureAwait(false);
                if (dbArtist == null)
                {
                    return new MelodeeModels.OperationResult<bool>($"Unknown artist to merge [{artistApiKeyToMerge}].")
                    {
                        Data = false
                    };
                }

                artistAlternateNamesToMerge.Add(dbArtist.NameNormalized);
                artistAlternateNamesToMerge.AddRange(dbArtist.AlternateNames.ToTags() ?? []);

                var artistPinType = (int)UserPinType.Artist;
                var userPins = await scopedContext.UserPins
                    .Where(x => x.PinType == artistPinType && x.PinId == dbArtist.Id)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
                foreach (var userPin in userPins)
                {
                    userPin.PinId = dbArtistToMergeInto.Id;
                    userPin.LastUpdatedAt = now;
                }

                foreach (var albumToMerge in dbArtist.Albums)
                {
                    try
                    {
                        var albumToMergeDirectory = Path.Combine(dbArtist.Library.Path, dbArtist.Directory,
                            albumToMerge.Directory);
                        var albumToMergeNewDirectory = Path.Combine(dbArtistToMergeInto.Library.Path,
                            dbArtistToMergeInto.Directory, albumToMerge.Directory);
                        if (fileSystemService.DirectoryExists(albumToMergeDirectory) && !fileSystemService.DirectoryExists(albumToMergeNewDirectory))
                        {
                            fileSystemService.MoveDirectory(albumToMergeDirectory, albumToMergeNewDirectory);
                        }
                        else if (fileSystemService.DirectoryExists(albumToMergeNewDirectory))
                        {
                            var albumJsonFiles = fileSystemService.GetFiles(albumToMergeNewDirectory,
                                MelodeeModels.Album.JsonFileName, SearchOption.TopDirectoryOnly);
                            if (albumJsonFiles.Length > 0)
                            {
                                var album = await fileSystemService.DeserializeAlbumAsync(albumJsonFiles[0],
                                        cancellationToken).ConfigureAwait(false);
                                if (album != null)
                                {
                                    await ProcessExistingDirectoryMoveMergeAsync(configuration, serializer, album,
                                        albumToMergeDirectory, cancellationToken).ConfigureAwait(false);
                                }
                            }
                        }

                        albumToMerge.ArtistId = dbArtistToMergeInto.Id;
                        albumToMerge.LastUpdatedAt = now;
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, "Error attempting to merge album [{Album}] into artist [{Artist}]",
                            albumToMerge.Directory, dbArtistToMergeInto.Name);
                    }
                }

                foreach (var userArtistToMerge in dbArtist.UserArtists)
                {
                    userArtistToMerge.ArtistId = dbArtistToMergeInto.Id;
                    userArtistToMerge.LastUpdatedAt = now;
                }

                await scopedContext.Contributors
                    .Where(x => x.ArtistId == dbArtist.Id)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(x => x.ArtistId, dbArtistToMergeInto.Id), cancellationToken)
                    .ConfigureAwait(false);

                scopedContext.Artists.Remove(dbArtist);

                var saveResult = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                if (saveResult > 0)
                {
                    var dbArtistDirectory = dbArtist.ToFileSystemDirectoryInfo();
                    if ((dbArtistToMergeInto.ImageCount ?? 0) == 0 && fileSystemService.DirectoryExists(dbArtistDirectory.FullName()))
                    {
                        dbArtistToMergeInto.ImageCount = dbArtistToMergeInto.ImageCount ?? 0;
                        var jpgFiles = fileSystemService.GetFiles(dbArtistDirectory.FullName(), "*.jpg", SearchOption.TopDirectoryOnly);
                        foreach (var jpgFile in jpgFiles)
                        {
                            var fileName = fileSystemService.GetFileName(jpgFile);
                            var newPath = fileSystemService.CombinePath(dbArtistToMergeIntoDirectoryPath, fileName);
                            fileSystemService.MoveDirectory(jpgFile, newPath);
                            dbArtistToMergeInto.ImageCount++;
                        }

                        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    }

                    if (fileSystemService.DirectoryExists(dbArtistDirectory.FullName()))
                    {
                        fileSystemService.DeleteDirectory(dbArtistDirectory.FullName(), true);
                    }
                }

                libraryIdsToUpdate.Add(dbArtist.Library.Id);
            }

            if (dbArtistToMergeInto.AlternateNames == null)
            {
                artistAlternateNamesToMerge.AddRange(dbArtistToMergeInto.AlternateNames.ToTags() ?? []);
            }

            dbArtistToMergeInto.AlternateNames = "".AddTags(artistAlternateNamesToMerge.Distinct(), doNormalize: true);

            await UpdateArtistAggregateValuesByIdAsync(dbArtistToMergeInto.Id, cancellationToken).ConfigureAwait(false);
            foreach (var libraryId in libraryIdsToUpdate.Distinct())
            {
                await UpdateLibraryAggregateStatsByIdAsync(libraryId, cancellationToken).ConfigureAwait(false);
            }

            // To clear the entire cache is unusual, but here we have (likely) deleted many artists, safer to clear all cache and let repopulate as needed.
            CacheManager.Clear();
            return new MelodeeModels.OperationResult<bool>
            {
                Data = true
            };
        }
    }

    public async Task<MelodeeModels.OperationResult<bool>> LockUnlockArtistAsync(
        int artistId, 
        bool doLock,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, artistId, nameof(artistId));

        var artistResult = await GetAsync(artistId, cancellationToken).ConfigureAwait(false);
        if (!artistResult.IsSuccess)
        {
            return new MelodeeModels.OperationResult<bool>($"Unknown artist to lock [{artistId}].")
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

    public async Task<MelodeeModels.OperationResult<bool>> DeleteAlbumsForArtist(
        int artistId, 
        int[] albumIdsToDelete,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, artistId, nameof(artistId));

        var artistResult = await GetAsync(artistId, cancellationToken).ConfigureAwait(false);
        if (!artistResult.IsSuccess)
        {
            return new MelodeeModels.OperationResult<bool>($"Unknown artist [{artistId}].")
            {
                Data = false
            };
        }

        var deleteResult = await albumService.DeleteAsync(albumIdsToDelete, cancellationToken).ConfigureAwait(false);
        var result = deleteResult.IsSuccess;
        if (deleteResult.IsSuccess)
        {
            await ClearCacheAsync(artistResult.Data!, cancellationToken);
        }

        return new MelodeeModels.OperationResult<bool>
        {
            Data = result
        };
    }
    
    public async Task<MelodeeModels.ImageBytesAndEtag> GetArtistImageBytesAndEtagAsync(Guid? apiKey, string? size = null, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(apiKey, nameof(apiKey));
        Guard.Against.Expression(x => x == Guid.Empty, apiKey!.Value, nameof(apiKey));

        var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        var artist = await GetByApiKeyAsync(apiKey.Value, cancellationToken).ConfigureAwait(false);
        if (!artist.IsSuccess || artist.Data == null)
        {
            return new MelodeeModels.ImageBytesAndEtag(null, null);
        }

        var artistId = artist.Data.Id;
        
        var cacheKey = CacheKeyArtistImageBytesAndEtagTemplate.FormatSmart(artistId, size ?? ImageSizeRegistry.Large);
        
        return await CacheManager.GetAsync(cacheKey, async () =>
        {
            var badEtag = Instant.MinValue.ToEtag();
            var sizeValue = size ?? ImageSizeRegistry.Large;

            var artistDirectory = artist.Data.ToFileSystemDirectoryInfo();
            if (!artistDirectory.Exists())
            {
                Logger.Warning("Artist directory [{Directory}] does not exist for artist [{ArtistId}].", artistDirectory.FullName(), artistId);
                return new MelodeeModels.ImageBytesAndEtag(null, badEtag);
            }

            var artistImages = artistDirectory.AllFileImageTypeFileInfos().ToArray();
            var imageFile = artistImages
                .FirstOrDefault(x => x.Name.Contains($"-{sizeValue}", StringComparison.OrdinalIgnoreCase) ||
                                     x.Name.Contains($"-{sizeValue}", StringComparison.OrdinalIgnoreCase)) ?? artistImages.OrderBy(x => x.Name).FirstOrDefault();

            if (imageFile is not { Exists: true })
            {
                Logger.Warning("No image found for artist [{ArtistId}] with size [{Size}].", artistId, sizeValue);
                return new MelodeeModels.ImageBytesAndEtag(null, badEtag);
            }

            var imageBytes = await fileSystemService.ReadAllBytesAsync(imageFile.FullName, cancellationToken).ConfigureAwait(false);
            return new MelodeeModels.ImageBytesAndEtag(imageBytes, (artist.Data.LastUpdatedAt ?? artist.Data.CreatedAt).ToEtag());
        }, cancellationToken, configuration.CacheDuration(), Artist.CacheRegion);
    }
}
