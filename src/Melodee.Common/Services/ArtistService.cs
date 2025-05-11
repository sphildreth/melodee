using Ardalis.GuardClauses;
using Dapper;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.MessageBus.Events;
using Melodee.Common.Models.Collection;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Plugins.Conversion.Image;
using Melodee.Common.Serialization;
using Melodee.Common.Services.Extensions;
using Melodee.Common.Services.Interfaces;
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
    IBus bus)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    private const string CacheKeyDetailByApiKeyTemplate = "urn:artist:apikey:{0}";
    private const string CacheKeyDetailByNameNormalizedTemplate = "urn:artist:namenormalized:{0}";
    private const string CacheKeyDetailByMusicBrainzIdTemplate = "urn:artist:musicbrainzid:{0}";
    private const string CacheKeyDetailTemplate = "urn:artist:{0}";

    public async Task<MelodeeModels.PagedResult<ArtistDataInfo>> ListAsync(MelodeeModels.PagedRequest pagedRequest,
        CancellationToken cancellationToken = default)
    {
        int artistCount;
        ArtistDataInfo[] artists = [];
        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var orderBy = pagedRequest.OrderByValue();
            var dbConn = scopedContext.Database.GetDbConnection();
            var countSqlParts = pagedRequest.FilterByParts("SELECT COUNT(*) FROM \"Artists\"");
            artistCount = await dbConn
                .QuerySingleAsync<int>(countSqlParts.Item1, countSqlParts.Item2)
                .ConfigureAwait(false);
            if (!pagedRequest.IsTotalCountOnlyRequest)
            {
                var sqlStartFragment = """
                                       SELECT a."Id", a."ApiKey", a."IsLocked", a."LibraryId", l."Path" as "LibraryPath", a."Name", a."NameNormalized", a."AlternateNames",
                                              a."Directory", a."AlbumCount", a."SongCount", a."CreatedAt", a."Tags", a."LastUpdatedAt"
                                       FROM "Artists" a
                                       JOIN "Libraries" l ON (a."LibraryId" = l."Id") 
                                       """;
                var listSqlParts = pagedRequest.FilterByParts(sqlStartFragment, "a");
                var listSql =
                    $"{listSqlParts.Item1} ORDER BY {orderBy} OFFSET {pagedRequest.SkipValue} ROWS FETCH NEXT {pagedRequest.TakeValue} ROWS ONLY;";
                artists = (await dbConn
                    .QueryAsync<ArtistDataInfo>(listSql, listSqlParts.Item2)
                    .ConfigureAwait(false)).ToArray();
            }
        }

        return new MelodeeModels.PagedResult<ArtistDataInfo>
        {
            TotalCount = artistCount,
            TotalPages = pagedRequest.TotalPages(artistCount),
            Data = artists
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
                await using (var scopedContext =
                             await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
                {
                    var dbConn = scopedContext.Database.GetDbConnection();
                    return await dbConn
                        .QuerySingleOrDefaultAsync<int?>(
                            "SELECT \"Id\" FROM \"Artists\" WHERE \"MusicBrainzId\" = @musicBrainzId",
                            new { musicBrainzId })
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

    public async Task<MelodeeModels.OperationResult<Artist?>> GetByNameNormalized(string nameNormalized,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrEmpty(nameNormalized, nameof(nameNormalized));

        var id = await CacheManager.GetAsync(CacheKeyDetailByNameNormalizedTemplate.FormatSmart(nameNormalized),
            async () =>
            {
                await using (var scopedContext =
                             await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
                {
                    var dbConn = scopedContext.Database.GetDbConnection();
                    return await dbConn
                        .QuerySingleOrDefaultAsync<int?>(
                            "SELECT \"Id\" FROM \"Artists\" WHERE \"NameNormalized\" = @nameNormalized",
                            new { nameNormalized })
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


    /// <summary>
    ///     Find the Artist using various given Ids.
    /// </summary>
    public async Task<MelodeeModels.OperationResult<Artist?>> FindArtistAsync(int? byId, Guid byApiKey, string? byName,
        Guid? byMusicBrainzId, string? bySpotifyId, CancellationToken cancellationToken = default)
    {
        int? id = null;

        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbConn = scopedContext.Database.GetDbConnection();
            try
            {
                var sql = string.Empty;

                if (byId.HasValue)
                {
                    sql = """
                           select a."Id"
                           from "Artists" a 
                           where a."Id" = @id
                          """;
                    id = await dbConn
                        .QuerySingleOrDefaultAsync<int?>(sql, new { id = byId })
                        .ConfigureAwait(false);
                }

                if (id == null && byApiKey != Guid.Empty)
                {
                    sql = """
                           select a."Id"
                           from "Artists" a 
                           where a."ApiKey" = @apiKey
                          """;
                    id = await dbConn
                        .QuerySingleOrDefaultAsync<int?>(sql, new { apiKey = byApiKey })
                        .ConfigureAwait(false);
                }

                if (id == null && (byMusicBrainzId != null || bySpotifyId != null))
                {
                    sql = """
                          select a."Id"
                          from "Artists" a 
                          where a."MusicBrainzId" = @musicBrainzId  
                          or a."SpotifyId" = @spotifyId
                          """;
                    id = await dbConn
                        .QuerySingleOrDefaultAsync<int?>(sql,
                            new { musicBrainzId = byMusicBrainzId, spotifyId = bySpotifyId })
                        .ConfigureAwait(false);
                }

                if (id == null)
                {
                    sql = """
                          select a."Id"
                          from "Artists" a 
                          where a."NameNormalized" = @name
                          """;
                    id = await dbConn
                        .QuerySingleOrDefaultAsync<int?>(sql, new { name = byName })
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
        Guard.Against.Expression(x => apiKey == Guid.Empty, apiKey, nameof(apiKey));

        var id = await CacheManager.GetAsync(CacheKeyDetailByApiKeyTemplate.FormatSmart(apiKey), async () =>
        {
            await using (var scopedContext =
                         await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var dbConn = scopedContext.Database.GetDbConnection();
                return await dbConn
                    .QuerySingleOrDefaultAsync<int?>("SELECT \"Id\" FROM \"Artists\" WHERE \"ApiKey\" = @apiKey",
                        new { apiKey })
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
            CacheManager.Remove(
                CacheKeyDetailByMusicBrainzIdTemplate.FormatSmart(artist.MusicBrainzId.Value.ToString()));
        }
    }

    public async Task ClearCacheAsync(int artistId, CancellationToken cancellationToken)
    {
        var artist = await GetAsync(artistId, cancellationToken).ConfigureAwait(false);
        ClearCache(artist.Data!);
    }

    public async Task<MelodeeModels.OperationResult<bool>> RescanAsync(int[] artistIds,
        CancellationToken cancellationToken = default)
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
                Path.Combine(artistResult.Data.Library.Path, artistResult.Data.Directory))).ConfigureAwait(false);
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
                if (Directory.Exists(artistDirectory))
                {
                    Directory.Delete(artistDirectory, true);
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
            dbDetail.AmgId = artist.AmgId;
            dbDetail.Biography = artist.Biography.Nullify();
            dbDetail.Description = artist.Description;
            dbDetail.Directory = artist.Directory;
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
                ClearCache(dbDetail);
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

    public async Task<MelodeeModels.OperationResult<bool>> SaveImageAsArtistImageAsync(int artistId,
        bool deleteAllImages, byte[] imageBytes, CancellationToken cancellationToken = default)
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
            Data = await SaveImageBytesAsArtistImageAsync(artist.Data, deleteAllImages, imageBytes, cancellationToken)
                .ConfigureAwait(false)
        };
    }

    private async Task<bool> SaveImageBytesAsArtistImageAsync(Artist artist, bool deleteAllImages, byte[] imageBytes,
        CancellationToken cancellationToken = default)
    {
        var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken);
        var imageConvertor = new ImageConvertor(configuration);
        var artistDirectory = artist.ToFileSystemDirectoryInfo();
        var artistImages = artistDirectory.FileInfosForExtension("jpg").ToArray();
        if (deleteAllImages && artistImages.Length != 0)
        {
            foreach (var fileInAlbumDirectory in artistImages)
            {
                if (fileInAlbumDirectory.Name.Contains(PictureIdentifier.Artist.ToString(),
                        StringComparison.OrdinalIgnoreCase) ||
                    fileInAlbumDirectory.Name.Contains(PictureIdentifier.ArtistSecondary.ToString(),
                        StringComparison.OrdinalIgnoreCase))
                {
                    fileInAlbumDirectory.Delete();
                }
            }

            artistImages = artistDirectory.FileInfosForExtension("jpg").ToArray();
        }

        var artistImageFileName = Path.Combine(artistDirectory.Path,
            deleteAllImages ? "01-Band.image" : $"{artistImages.Length + 1}-Band.image");
        var artistImageFileInfo = new FileInfo(artistImageFileName).ToFileSystemInfo();
        await File.WriteAllBytesAsync(artistImageFileInfo.FullName(artistDirectory), imageBytes, cancellationToken);
        await imageConvertor.ProcessFileAsync(
            artistDirectory,
            artistImageFileInfo,
            cancellationToken);
        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
            await scopedContext.Artists
                .Where(x => x.Id == artist.Id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.LastUpdatedAt, now)
                    .SetProperty(x => x.ImageCount, artistImages.Length + 1), cancellationToken)
                .ConfigureAwait(false);
        }

        ClearCache(artist);
        OpenSubsonicApiService.ClearImageCacheForApiId(artist.ToApiKey(), CacheManager);
        return true;
    }

    public async Task<MelodeeModels.OperationResult<bool>> SaveImageUrlAsArtistImageAsync(int artistId, string imageUrl,
        bool deleteAllImages, CancellationToken cancellationToken = default)
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
                configuration.GetValue<string?>(SettingRegistry.SearchEngineUserAgent) ?? string.Empty, imageUrl,
                cancellationToken);
            if (imageBytes != null)
            {
                result = await SaveImageBytesAsArtistImageAsync(artist.Data, deleteAllImages, imageBytes,
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

            var dbArtistToMergeIntoDirectory = dbArtistToMergeInto.ToFileSystemDirectoryInfo();
            if (!Directory.Exists(dbArtistToMergeIntoDirectory.FullName()))
            {
                Directory.CreateDirectory(dbArtistToMergeIntoDirectory.FullName());
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
                        if (Directory.Exists(albumToMergeDirectory) && !Directory.Exists(albumToMergeNewDirectory))
                        {
                            albumToMergeDirectory.ToDirectoryInfo().MoveToDirectory(albumToMergeNewDirectory);
                        }
                        else if (Directory.Exists(albumToMergeNewDirectory))
                        {
                            var albumJsonFiles = Directory.GetFiles(albumToMergeNewDirectory,
                                MelodeeModels.Album.JsonFileName, SearchOption.TopDirectoryOnly);
                            if (albumJsonFiles.Length > 0)
                            {
                                var album = await MelodeeModels.Album
                                    .DeserializeAndInitializeAlbumAsync(serializer, albumJsonFiles[0],
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
                    if ((dbArtistToMergeInto.ImageCount ?? 0) == 0 && Directory.Exists(dbArtistDirectory.FullName()))
                    {
                        dbArtistToMergeInto.ImageCount = dbArtistToMergeInto.ImageCount ?? 0;
                        foreach (var dbArtistImage in dbArtistDirectory.FileInfosForExtension("jpg"))
                        {
                            dbArtistImage.MoveTo(Path.Combine(dbArtistToMergeIntoDirectory.FullName(),
                                dbArtistImage.Name));
                            dbArtistToMergeInto.ImageCount++;
                        }

                        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    }

                    var dirPath = dbArtist.ToFileSystemDirectoryInfo().FullName();
                    if (Directory.Exists(dirPath))
                    {
                        Directory.Delete(dirPath, true);
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

            // To clear the entire cache is unusual, but here we have deleted (likely) many artists, safer to clear all cache and let repopulate as needed.
            CacheManager.Clear();
            return new MelodeeModels.OperationResult<bool>
            {
                Data = true
            };
        }
    }

    public async Task<MelodeeModels.OperationResult<bool>> LockUnlockArtistAsync(int artistId, bool doLock,
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
        var result = (await UpdateAsync(artistResult.Data, cancellationToken).ConfigureAwait(false))?.Data ?? false;
        return new MelodeeModels.OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<bool>> DeleteAlbumsForArtist(int artistId, int[] albumIdsToDelete,
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
            ClearCache(artistResult.Data!);
        }

        return new MelodeeModels.OperationResult<bool>
        {
            Data = result
        };
    }
}
