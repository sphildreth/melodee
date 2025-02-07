using System.Collections.Concurrent;
using System.Diagnostics;
using Ardalis.GuardClauses;
using Dapper;
using IdSharp.Common.Utils;
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
using Melodee.Common.Plugins.Validation;
using Melodee.Common.Serialization;
using Melodee.Common.Services.Interfaces;
using Melodee.Common.Services.Models;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Rebus.Bus;
using Serilog;
using SmartFormat;
using MelodeeModels = Melodee.Common.Models;
using SearchOption = System.IO.SearchOption;

namespace Melodee.Common.Services;

public class LibraryService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    IMelodeeConfigurationFactory configurationFactory,
    ISerializer serializer,
    IBus bus)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    private const string CacheKeyDetailByApiKeyTemplate = "urn:library:apikey:{0}";
    private const string CacheKeyDetailLibraryByType = "urn:library_by_type:{0}";
    private const string CacheKeyDetailTemplate = "urn:library:{0}";
    private const string CacheKeyMediaLibraries = "urn:libraries:media-libraries";

    private const int DisplayNumberPadLength = 8;

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

    public async Task<MelodeeModels.OperationResult<Library[]>> GetStorageLibrariesAsync(CancellationToken cancellationToken = default)
    {
        const int libraryType = (int)LibraryType.Storage;
        var result = await CacheManager.GetAsync(CacheKeyDetailLibraryByType.FormatSmart(libraryType), async () =>
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var storageLibraryType = (int)LibraryType.Storage;
                var library = await scopedContext
                    .Libraries
                    .Where(x => x.Type == storageLibraryType)
                    .ToArrayAsync(cancellationToken)
                    .ConfigureAwait(false);
                if (library == null)
                {
                    throw new Exception("No storage library found. At least one Library record must be setup with a type of '3' (Storage).");
                }

                return library;
            }
        }, cancellationToken).ConfigureAwait(false);
        return new MelodeeModels.OperationResult<Library[]>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<Library>> GetUserImagesLibraryAsync(CancellationToken cancellationToken = default)
    {
        const int libraryType = (int)LibraryType.UserImages;
        var result = await CacheManager.GetAsync(CacheKeyDetailLibraryByType.FormatSmart(libraryType), async () =>
        {
            var library = await LibraryByType(libraryType, cancellationToken);
            if (library == null)
            {
                throw new Exception("User Images library not found. A Library record must be setup with a type of '4' (UserImages).");
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

        var id = await CacheManager.GetAsync(CacheKeyDetailByApiKeyTemplate.FormatSmart(apiKey), async () =>
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

    // public virtual async Task<MelodeeModels.OperationResult<Library>> GetLibraryAsync(CancellationToken cancellationToken = default)
    // {
    //     const int libraryType = (int)LibraryType.Storage;
    //     var result = await CacheManager.GetAsync(CacheKeyDetailLibraryByType.FormatSmart(libraryType), async () =>
    //     {
    //         var library = await LibraryByType(libraryType, cancellationToken);
    //         if (library == null)
    //         {
    //             throw new Exception("Library not found. A Library record must be setup with a type of '3' (Library).");
    //         }
    //
    //         return library;
    //     }, cancellationToken);
    //     return new MelodeeModels.OperationResult<Library>
    //     {
    //         Data = result
    //     };
    // }

    public async Task<MelodeeModels.OperationResult<Library?>> PurgeLibraryAsync(int libraryId, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, libraryId, nameof(libraryId));

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
                    Type = MelodeeModels.OperationResponseType.Error
                };
            }

            dbLibrary.PurgePath();

            await scopedContext
                .Artists
                .Where(x => x.LibraryId == libraryId)
                .ExecuteDeleteAsync(cancellationToken)
                .ConfigureAwait(false);

            await scopedContext
                .LibraryScanHistories
                .Where(x => x.LibraryId == libraryId)
                .ExecuteDeleteAsync(cancellationToken)
                .ConfigureAwait(false);

            dbLibrary.ArtistCount = 0;
            dbLibrary.AlbumCount = 0;
            dbLibrary.SongCount = 0;
            dbLibrary.LastScanAt = null;
            dbLibrary.LastUpdatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow);

            await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            ClearCache(dbLibrary);
        }

        return await GetAsync(libraryId, cancellationToken);
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

    public async Task<MelodeeModels.PagedResult<LibraryScanHistoryDataInfo>> ListLibraryHistoriesAsync(int libraryId, MelodeeModels.PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        var librariesCount = 0;
        LibraryScanHistoryDataInfo[] histories = [];
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            try
            {
                var orderBy = pagedRequest.OrderByValue();
                var dbConn = scopedContext.Database.GetDbConnection();
                var countSqlParts = pagedRequest.FilterByParts("SELECT COUNT(*) FROM \"LibraryScanHistories\" where \"LibraryId\" = {0} ".FormatSmart(libraryId));
                librariesCount = await dbConn
                    .QuerySingleAsync<int>(countSqlParts.Item1, countSqlParts.Item2)
                    .ConfigureAwait(false);
                if (!pagedRequest.IsTotalCountOnlyRequest)
                {
                    var sqlStartFragment = """
                                           SELECT l."Id", l."CreatedAt", ar."Name" as "ForArtistName", ar."ApiKey" as "ForArtistApiKey", a."Name" as "ForAlbumName",
                                                  a."ApiKey" as "ForAlbumApiKey", l."FoundArtistsCount", l."FoundAlbumsCount", l."FoundSongsCount", l."DurationInMs"
                                           FROM "LibraryScanHistories" l
                                           left join "Artists" ar on (l."ForArtistId" = ar."Id")
                                           left join "Albums" a on (L."ForAlbumId" = a."Id")
                                           where l."LibraryId" = {0} 
                                           """;
                    var listSqlParts = pagedRequest.FilterByParts(sqlStartFragment.FormatSmart(libraryId));
                    var listSql = $"{listSqlParts.Item1} ORDER BY {orderBy} OFFSET {pagedRequest.SkipValue} ROWS FETCH NEXT {pagedRequest.TakeValue} ROWS ONLY;";
                    histories = (await dbConn
                        .QueryAsync<LibraryScanHistoryDataInfo>(listSql, listSqlParts.Item2)
                        .ConfigureAwait(false)).ToArray();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to get libraries from database");
            }
        }

        return new MelodeeModels.PagedResult<LibraryScanHistoryDataInfo>
        {
            TotalCount = librariesCount,
            TotalPages = pagedRequest.TotalPages(librariesCount),
            Data = histories
        };
    }

    public async Task<MelodeeModels.PagedResult<Library>> ListMediaLibrariesAsync(CancellationToken cancellationToken = default)
    {
        return await CacheManager.GetAsync(CacheKeyMediaLibraries, async () =>
        {
            var data = await ListAsync(new MelodeeModels.PagedRequest { PageSize = short.MaxValue }, cancellationToken).ConfigureAwait(false);
            return new MelodeeModels.PagedResult<Library>
            {
                Data = data
                    .Data
                    .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
                    .Where(x => x.TypeValue is LibraryType.Inbound or LibraryType.Staging)
                    .ToArray()
            };
        }, cancellationToken).ConfigureAwait(false);
    }


    public async Task<MelodeeModels.PagedResult<Library>> ListAsync(MelodeeModels.PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        var librariesCount = 0;
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

    public async Task<MelodeeModels.OperationResult<bool>> MoveAlbumsToLibrary(Library library, MelodeeModels.Album[] albums, CancellationToken cancellationToken = default)
    {
        var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken);
        var albumValidator = new AlbumValidator(configuration);

        var maxArtistImageCount = configuration.GetValue<short>(SettingRegistry.ImagingMaximumNumberOfArtistImages);
        var movedCount = 0;
        foreach (var album in albums)
        {
            if (!albumValidator.ValidateAlbum(album).Data.IsValid)
            {
                Logger.Debug("[{ServiceName}] Not moving invalid album [{Album}]", nameof(LibraryService), album.ToString());
                continue;
            }

            var artistDirectory = album.Artist.ToDirectoryName(configuration.GetValue<short>(SettingRegistry.ProcessingMaximumArtistDirectoryNameLength));
            var albumDirectory = album.AlbumDirectoryName(configuration.Configuration);
            var libraryAlbumPath = Path.Combine(library.Path, artistDirectory, albumDirectory);
            if (!Directory.Exists(libraryAlbumPath))
            {
                Directory.CreateDirectory(libraryAlbumPath);
            }
            else
            {
                var processExistingDirectoryResult = await ProcessExistingDirectoryMoveMergeAsync(configuration, serializer, album, libraryAlbumPath, cancellationToken).ConfigureAwait(false);
                if (processExistingDirectoryResult != null)
                {
                    await bus.SendLocal(new AlbumUpdatedEvent(processExistingDirectoryResult.AlbumId, processExistingDirectoryResult.AlbumPath)).ConfigureAwait(false);
                }

                continue;
            }

            var libraryArtistDirectoryInfo = new DirectoryInfo(Path.Combine(library.Path, artistDirectory)).ToDirectorySystemInfo();
            var libraryAlbumDirectoryInfo = new DirectoryInfo(libraryAlbumPath).ToDirectorySystemInfo();
            album.Directory.MoveToDirectory(libraryAlbumPath);
            var melodeeFileName = Path.Combine(libraryAlbumPath, "melodee.json");
            var melodeeFile = await MelodeeModels.Album.DeserializeAndInitializeAlbumAsync(serializer, melodeeFileName, cancellationToken).ConfigureAwait(false);
            melodeeFile!.Directory.Path = libraryAlbumPath;
            if (album.Artist.Images?.Any() ?? false)
            {
                var existingArtistImages = libraryArtistDirectoryInfo.AllFileImageTypeFileInfos().Where(x => ImageHelper.IsArtistImage(x) || ImageHelper.IsArtistSecondaryImage(x)).ToArray();
                if (existingArtistImages.Length == 0)
                {
                    // If there are no artist images in artists library directory, move artist images from album directory
                    foreach (var image in album.Artist.Images)
                    {
                        if (image.FileInfo != null)
                        {
                            var fileToMoveFullName = Path.Combine(libraryAlbumDirectoryInfo.FullName(), image.FileInfo.Name);
                            File.Move(fileToMoveFullName, image.FileInfo.FullName(libraryArtistDirectoryInfo));
                            Logger.Information("[{ServiceName}] moved artist image [{ImageName}] into artist directory", nameof(LibraryService), fileToMoveFullName);
                            movedCount++;
                        }
                    }
                }
                else
                {
                    var existingArtistImagesCrc32S = existingArtistImages.Select(Crc32.Calculate).ToArray();
                    foreach (var image in album.Artist.Images.ToArray())
                    {
                        if (image.FileInfo != null)
                        {
                            // If there are artist images, check CRC and see if duplicate, delete any duplicate found in album directory
                            if (existingArtistImagesCrc32S.Contains(CRC32.Calculate(image.FileInfo.ToFileInfo(libraryArtistDirectoryInfo))))
                            {
                                var fileToDeleteFullName = Path.Combine(libraryArtistDirectoryInfo.FullName(), image.FileInfo.Name);
                                File.Delete(fileToDeleteFullName);
                                Logger.Information("[{ServiceName}] deleted duplicate artist image [{ImageName}]", nameof(LibraryService), fileToDeleteFullName);
                            }
                            else
                            {
                                var fileToMoveFullName = Path.Combine(libraryAlbumDirectoryInfo.FullName(), image.FileInfo.Name);
                                var moveToFileFullName = Path.Combine(libraryArtistDirectoryInfo.FullName(), libraryArtistDirectoryInfo.GetNextFileNameForType(maxArtistImageCount, Artist.ImageType).Item1);
                                File.Move(fileToMoveFullName, moveToFileFullName);
                                Logger.Information("[{ServiceName}] moved artist image [{ImageName}] into artist directory", nameof(LibraryService), fileToMoveFullName);

                                movedCount++;
                            }
                        }
                    }
                }

                melodeeFile.Artist = melodeeFile.Artist with { Images = null };
            }

            await File.WriteAllTextAsync(melodeeFileName, serializer.Serialize(melodeeFile), cancellationToken);

            movedCount++;

            OnProcessingProgressEvent?.Invoke(this,
                new ProcessingEvent(ProcessingEventType.Processing,
                    nameof(MoveAlbumsFromLibraryToLibrary),
                    albums.Count(),
                    movedCount,
                    $"Processing [{album}]"
                ));
        }

        return new MelodeeModels.OperationResult<bool>
        {
            Data = movedCount > 0
        };
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
                    Type = MelodeeModels.OperationResponseType.Error
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

            ClearCache(dbLibrary);
            return new MelodeeModels.OperationResult<LibraryScanHistory?>
            {
                Data = newLibraryScanHistory
            };
        }
    }

    public event EventHandler<ProcessingEvent>? OnProcessingProgressEvent;

    private async Task<Library?> LibraryByType(int type, CancellationToken cancellationToken = default)
    {
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbConn = scopedContext.Database.GetDbConnection();
            var sql = $"SELECT * FROM \"Libraries\" WHERE \"Type\" = {type} ORDER BY \"SortOrder\" LIMIT 1;";
            return await dbConn
                .QuerySingleOrDefaultAsync<Library?>(sql)
                .ConfigureAwait(false);
        }
    }

    private void ClearCache(Library library)
    {
        CacheManager.Remove(CacheKeyDetailByApiKeyTemplate.FormatSmart(library.ApiKey));
        CacheManager.Remove(CacheKeyDetailTemplate.FormatSmart(library.Id));
        CacheManager.Remove(CacheKeyDetailLibraryByType.FormatSmart(library.Type));
        CacheManager.Remove(CacheKeyMediaLibraries);
    }

    public async Task<MelodeeModels.OperationResult<bool>> MoveAlbumsFromLibraryToLibrary(string fromLibraryName, string toLibraryName, Func<MelodeeModels.Album, bool> condition, bool verboseSet, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrEmpty(fromLibraryName, nameof(fromLibraryName));
        Guard.Against.NullOrEmpty(fromLibraryName, nameof(toLibraryName));

        if (fromLibraryName.Equals(toLibraryName, StringComparison.OrdinalIgnoreCase))
        {
            return new MelodeeModels.OperationResult<bool>("From and To Library cannot be the same.")
            {
                Data = false
            };
        }

        var libraries = await ListAsync(new MelodeeModels.PagedRequest { PageSize = short.MaxValue }, cancellationToken).ConfigureAwait(false);
        var fromLibrary = libraries.Data.FirstOrDefault(x => x.Name.ToNormalizedString() == fromLibraryName.ToNormalizedString());
        if (fromLibrary == null)
        {
            return new MelodeeModels.OperationResult<bool>("Invalid From library Name")
            {
                Data = false
            };
        }

        if (fromLibrary.IsLocked)
        {
            return new MelodeeModels.OperationResult<bool>("From library is locked.")
            {
                Data = false
            };
        }

        var toLibrary = libraries.Data.FirstOrDefault(x => x.Name.ToNormalizedString() == toLibraryName.ToNormalizedString());
        if (toLibrary == null)
        {
            return new MelodeeModels.OperationResult<bool>("Invalid To library Name")
            {
                Data = false
            };
        }

        if (toLibrary.TypeValue != LibraryType.Storage)
        {
            return new MelodeeModels.OperationResult<bool>($"Invalid library type, this move process requires a library type of 'Library' ({(int)LibraryType.Storage}).")
            {
                Data = false
            };
        }

        if (toLibrary.IsLocked)
        {
            return new MelodeeModels.OperationResult<bool>("To library is locked.")
            {
                Data = false
            };
        }

        var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken);
        var albumValidator = new AlbumValidator(configuration);

        var duplicateDirPrefix = configuration.GetValue<string>(SettingRegistry.ProcessingDuplicateAlbumPrefix);
        var maxAlbumProcessingCount = configuration.GetValue<int>(SettingRegistry.ProcessingMaximumProcessingCount, value => value < 1 ? int.MaxValue : value);
        var albumsForFromLibrary = Directory.GetFiles(fromLibrary.Path, MelodeeModels.Album.JsonFileName, SearchOption.AllDirectories);
        var albumsToMove = new List<MelodeeModels.Album>();
        foreach (var albumFile in albumsForFromLibrary)
        {
            var dirInfo = new DirectoryInfo(Path.GetDirectoryName(albumFile) ?? string.Empty);
            if (!dirInfo.Exists)
            {
                continue;
            }

            if (duplicateDirPrefix != null)
            {
                if (dirInfo.Name.StartsWith(duplicateDirPrefix))
                {
                    continue;
                }
            }

            var album = await MelodeeModels.Album.DeserializeAndInitializeAlbumAsync(serializer, albumFile, cancellationToken).ConfigureAwait(false);
            if (album != null)
            {
                if (!albumValidator.ValidateAlbum(album).Data.IsValid)
                {
                    continue;
                }

                if (condition(album))
                {
                    albumsToMove.Add(album);
                }
            }

            if (albumsToMove.Count >= maxAlbumProcessingCount)
            {
                break;
            }
        }

        var numberOfAlbumsToMove = albumsToMove.Count();

        OnProcessingProgressEvent?.Invoke(this,
            new ProcessingEvent(ProcessingEventType.Start,
                nameof(MoveAlbumsFromLibraryToLibrary),
                numberOfAlbumsToMove,
                0,
                "Starting processing"
            ));

        var result = await MoveAlbumsToLibrary(toLibrary, albumsToMove.ToArray(), cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            return new MelodeeModels.OperationResult<bool>(result.Messages)
            {
                Data = false,
                Errors = result.Errors
            };
        }

        OnProcessingProgressEvent?.Invoke(this,
            new ProcessingEvent(ProcessingEventType.Stop,
                nameof(MoveAlbumsFromLibraryToLibrary),
                numberOfAlbumsToMove,
                numberOfAlbumsToMove,
                "Completed processing"
            ));
        return new MelodeeModels.OperationResult<bool>
        {
            Data = true
        };
    }

    public async Task<MelodeeModels.OperationResult<MelodeeModels.Statistic[]?>> AlbumStatusReport(string libraryName, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrEmpty(libraryName, nameof(libraryName));

        var libraries = await ListAsync(new MelodeeModels.PagedRequest { PageSize = short.MaxValue }, cancellationToken).ConfigureAwait(false);
        var library = libraries.Data.FirstOrDefault(x => x.Name.ToNormalizedString() == libraryName.ToNormalizedString());
        if (library == null)
        {
            return new MelodeeModels.OperationResult<MelodeeModels.Statistic[]?>("Invalid library Name")
            {
                Data = []
            };
        }

        var result = new List<MelodeeModels.Statistic>();

        // Get all melodee albums in library path
        var libraryDirectory = new DirectoryInfo(library.Path);
        var melodeeFileSystemInfosForLibrary = libraryDirectory.GetFileSystemInfos(MelodeeModels.Album.JsonFileName, SearchOption.AllDirectories).ToArray();
        if (melodeeFileSystemInfosForLibrary.Length == 0)
        {
            return new MelodeeModels.OperationResult<MelodeeModels.Statistic[]?>("Library has no albums.")
            {
                Data = []
            };
        }

        var melodeeFilesForLibrary = new List<MelodeeModels.Album>();
        foreach (var melodeeFileSystemInfo in melodeeFileSystemInfosForLibrary)
        {
            var melodeeFile = await MelodeeModels.Album.DeserializeAndInitializeAlbumAsync(serializer, melodeeFileSystemInfo.FullName, cancellationToken).ConfigureAwait(false);
            if (melodeeFile != null)
            {
                melodeeFilesForLibrary.Add(melodeeFile);
            }
        }

        Trace.WriteLine($"Found [{melodeeFilesForLibrary.Count}] albums in library [{library}]...");

        var melodeeFilesGrouped = melodeeFilesForLibrary.GroupBy(x => x.Status);
        var melodeeFilesGroupedOk = melodeeFilesGrouped.FirstOrDefault(x => x.Key == AlbumStatus.Ok);
        if (melodeeFilesGroupedOk != null)
        {
            result.Add(new MelodeeModels.Statistic(StatisticType.Information, melodeeFilesGroupedOk.Key.ToString(), melodeeFilesGroupedOk.Count(), StatisticColorRegistry.Ok, "Album with `Ok` status."));
            var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken);
            var albumValidator = new AlbumValidator(configuration);
            foreach (var album in melodeeFilesGroupedOk)
            {
                var validateResults = albumValidator.ValidateAlbum(album);
                if (!validateResults.Data.IsValid)
                {
                    result.Add(new MelodeeModels.Statistic(StatisticType.Warning, $"Album with `Ok` status [{album}], is invalid", serializer.Serialize(validateResults.Data) ?? string.Empty, StatisticColorRegistry.Warning));
                }
            }
        }

        foreach (var album in melodeeFilesForLibrary.Where(x => x.Status != AlbumStatus.Ok))
        {
            result.Add(new MelodeeModels.Statistic(StatisticType.Warning, album.Directory.Name, album.StatusReasons.ToString(), StatisticColorRegistry.Warning));
        }

        return new MelodeeModels.OperationResult<MelodeeModels.Statistic[]?>
        {
            Data = result.ToArray()
        };
    }

    public async Task<MelodeeModels.OperationResult<MelodeeModels.Statistic[]?>> Statistics(string settingsLibraryName, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrEmpty(settingsLibraryName, nameof(settingsLibraryName));

        var libraries = await ListAsync(new MelodeeModels.PagedRequest { PageSize = short.MaxValue }, cancellationToken).ConfigureAwait(false);
        var library = libraries.Data.FirstOrDefault(x => x.Name.ToNormalizedString() == settingsLibraryName.ToNormalizedString());
        if (library == null)
        {
            return new MelodeeModels.OperationResult<MelodeeModels.Statistic[]?>("Invalid From library Name")
            {
                Data = []
            };
        }

        var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken);
        var duplicateDirPrefix = configuration.GetValue<string>(SettingRegistry.ProcessingDuplicateAlbumPrefix);

        var result = new List<MelodeeModels.Statistic>
        {
            new(StatisticType.Information, "Artist Count", library.ArtistCount.ToStringPadLeft(DisplayNumberPadLength) ?? "0", StatisticColorRegistry.Ok, "Number of artists on Library db record."),
            new(StatisticType.Information, "Album Count", library.AlbumCount.ToStringPadLeft(DisplayNumberPadLength) ?? "0", StatisticColorRegistry.Ok, "Number of albums on Library db record."),
            new(StatisticType.Information, "Song Count", library.SongCount.ToStringPadLeft(DisplayNumberPadLength) ?? "0", StatisticColorRegistry.Ok, "Number of songs on Library db record.")
        };

        if (library.TypeValue == LibraryType.Storage)
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var artistDirectoriesFound = 0;
                var albumDirectoriesFound = 0;
                var albumDirectoriesWithoutMelodeeDataFiles = new List<string>();
                var songsFound = 0;
                var allDirectoriesInLibrary = Directory.GetDirectories(library.Path, "*", SearchOption.TopDirectoryOnly).ToArray();
                foreach (var directory in allDirectoriesInLibrary)
                {
                    var d = new DirectoryInfo(directory);

                    if (duplicateDirPrefix != null)
                    {
                        if (d.Name.StartsWith(duplicateDirPrefix))
                        {
                            continue;
                        }
                    }

                    if (d.Name.Length == 1)
                    {
                        foreach (var letterDirectory in Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly))
                        {
                            d = new DirectoryInfo(letterDirectory);
                            if (d.Name.Length == 2)
                            {
                                foreach (var artistDirectory in Directory.GetDirectories(letterDirectory, "*", SearchOption.TopDirectoryOnly))
                                {
                                    d = new DirectoryInfo(artistDirectory);
                                    var dPath = $"{d.FullName.Replace(library.Path, string.Empty)}/";
                                    var dbArtist = await scopedContext.Artists.FirstOrDefaultAsync(x => x.Directory == dPath, cancellationToken).ConfigureAwait(false);
                                    if (dbArtist == null)
                                    {
                                        result.Add(new MelodeeModels.Statistic(StatisticType.Error, "! Unknown artist directory", artistDirectory, StatisticColorRegistry.Error, $"Unable to find artist for directory [{d.Name}]"));
                                    }

                                    artistDirectoriesFound++;
                                    foreach (var albumDirectory in Directory.GetDirectories(artistDirectory, "*", SearchOption.TopDirectoryOnly))
                                    {
                                        d = new DirectoryInfo(albumDirectory);

                                        if (duplicateDirPrefix != null)
                                        {
                                            if (d.Name.StartsWith(duplicateDirPrefix))
                                            {
                                                continue;
                                            }
                                        }

                                        var melodeeFile = d.GetFileSystemInfos(MelodeeModels.Album.JsonFileName, SearchOption.TopDirectoryOnly).FirstOrDefault();
                                        if (melodeeFile == null)
                                        {
                                            Logger.Warning("Album directory [{DirName}] has no melodee data file.", d.FullName);
                                            continue;
                                        }

                                        try
                                        {
                                            await MelodeeModels.Album.DeserializeAndInitializeAlbumAsync(serializer, melodeeFile.FullName, cancellationToken).ConfigureAwait(false);
                                        }
                                        catch (Exception ex)
                                        {
                                            continue;
                                        }

                                        var aPath = $"{d.Name}/";
                                        var dbAlbum = await scopedContext
                                            .Albums.Include(x => x.Songs)
                                            .FirstOrDefaultAsync(x => x.Directory == aPath, cancellationToken)
                                            .ConfigureAwait(false);
                                        if (dbAlbum == null)
                                        {
                                            result.Add(new MelodeeModels.Statistic(StatisticType.Error, "! Unknown album directory", albumDirectory, StatisticColorRegistry.Error, $"Unable to find album for directory [{d.Name}]"));
                                            continue;
                                        }

                                        var albumSongsFound = 0;
                                        foreach (var songFound in d.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly).Where(x => FileHelper.IsFileMediaType(x.Extension)).OrderBy(x => x.Name))
                                        {
                                            var dbSong = dbAlbum?.Songs.FirstOrDefault(x => x.FileName == songFound.Name);
                                            if (dbSong == null)
                                            {
                                                result.Add(new MelodeeModels.Statistic(StatisticType.Error, "! Unknown song", songFound.Name, StatisticColorRegistry.Error, $"Album Id [{dbAlbum?.Id}]: Unable to find song for album"));
                                            }

                                            albumSongsFound++;
                                        }

                                        if (albumSongsFound != dbAlbum?.SongCount)
                                        {
                                            result.Add(new MelodeeModels.Statistic(StatisticType.Error, "! Album song count mismatch ", $"{dbArtist?.Directory}{d.Name}", StatisticColorRegistry.Error, $"Album Id [{dbAlbum?.Id}]: Found [{albumSongsFound.ToStringPadLeft(4)}] album has [{dbAlbum?.SongCount.ToStringPadLeft(4)}]"));
                                        }

                                        songsFound += albumSongsFound;

                                        if (!d.EnumerateFiles(MelodeeModels.Album.JsonFileName).Any())
                                        {
                                            albumDirectoriesWithoutMelodeeDataFiles.Add(d.FullName);
                                        }

                                        albumDirectoriesFound++;
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (var albumDirectoriesWithoutMelodeeDataFile in albumDirectoriesWithoutMelodeeDataFiles)
                {
                    result.Add(new MelodeeModels.Statistic(StatisticType.Error, "! Album directory missing Melodee data file", albumDirectoriesWithoutMelodeeDataFile, StatisticColorRegistry.Error, "When scanning media without a Melodee data file, media files will not get processed."));
                }

                var message = artistDirectoriesFound == library.ArtistCount ? null : $"Artist directory count [{artistDirectoriesFound.ToStringPadLeft(DisplayNumberPadLength)}] does not match Library artist count [{library.ArtistCount.ToStringPadLeft(DisplayNumberPadLength)}].";
                result.Add(new MelodeeModels.Statistic(artistDirectoriesFound == library.ArtistCount ? StatisticType.Information : StatisticType.Warning, "Artist Directories Found", artistDirectoriesFound.ToStringPadLeft(DisplayNumberPadLength), artistDirectoriesFound == library.ArtistCount ? StatisticColorRegistry.Ok : StatisticColorRegistry.Warning, message));

                message = albumDirectoriesFound == library.AlbumCount ? null : $"Album directory count [{albumDirectoriesFound}] does not match Library album count [{library.AlbumCount}].";
                result.Add(new MelodeeModels.Statistic(albumDirectoriesFound == library.AlbumCount ? StatisticType.Information : StatisticType.Warning, "Album Directories Found", albumDirectoriesFound.ToStringPadLeft(DisplayNumberPadLength), albumDirectoriesFound == library.AlbumCount ? StatisticColorRegistry.Ok : StatisticColorRegistry.Warning, message));

                message = songsFound == library.SongCount ? null : $"Song directory count [{songsFound.ToStringPadLeft(DisplayNumberPadLength)}] does not match Library song count [{library.SongCount.ToStringPadLeft(DisplayNumberPadLength)}].";
                result.Add(new MelodeeModels.Statistic(songsFound == library.SongCount ? StatisticType.Information : StatisticType.Error, "Songs Found", songsFound.ToStringPadLeft(DisplayNumberPadLength), songsFound == library.SongCount ? StatisticColorRegistry.Ok : StatisticColorRegistry.Warning, message));
            }
        }

        return new MelodeeModels.OperationResult<MelodeeModels.Statistic[]?>
        {
            Data = result.ToArray()
        };
    }

    public async Task<MelodeeModels.OperationResult<string[]>> CleanLibraryAsync(string libraryName, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrEmpty(libraryName, nameof(libraryName));

        var libraries = await ListAsync(new MelodeeModels.PagedRequest { PageSize = short.MaxValue }, cancellationToken).ConfigureAwait(false);
        var library = libraries.Data.FirstOrDefault(x => x.Name.ToNormalizedString() == libraryName.ToNormalizedString());
        if (library == null)
        {
            return new MelodeeModels.OperationResult<string[]>("Invalid from library Name")
            {
                Data = []
            };
        }

        if (library.IsLocked)
        {
            return new MelodeeModels.OperationResult<string[]>("Library is locked.")
            {
                Data = []
            };
        }

        var libDir = library.ToFileSystemDirectoryInfo();

        var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken);

        var maxNumberOfArtistImagesAllowed = configuration.GetValue<short>(SettingRegistry.ImagingMaximumNumberOfArtistImages);
        if (maxNumberOfArtistImagesAllowed == 0)
        {
            maxNumberOfArtistImagesAllowed = short.MaxValue;
        }

        var maxNumberOfAlbumImagesAllowed = configuration.GetValue<short>(SettingRegistry.ImagingMaximumNumberOfAlbumImages);
        if (maxNumberOfAlbumImagesAllowed == 0)
        {
            maxNumberOfAlbumImagesAllowed = short.MaxValue;
        }

        var messages = new List<string>();
        var allDirectoriesInLibrary = libDir.AllDirectoryInfos(searchOption: SearchOption.TopDirectoryOnly).ToArray();
        Trace.WriteLine($"Found [{allDirectoriesInLibrary.Length}] top level directories...");
        var libraryDirectoryCountBeforeCleaning = allDirectoriesInLibrary.Length;

        // Look for images and delete directories that don't have any media files
        var directoriesWithoutMediaFiles = new ConcurrentBag<MelodeeModels.FileSystemDirectoryInfo>();
        Parallel.ForEach(allDirectoriesInLibrary, directory =>
        {
            var dd = directory.ToDirectorySystemInfo();
            if (!dd.DoesDirectoryHaveMediaFiles())
            {
                directoriesWithoutMediaFiles.Add(dd);
            }

            ;
        });
        if (directoriesWithoutMediaFiles.Distinct().Any())
        {
            Trace.WriteLine($"Found [{directoriesWithoutMediaFiles.Count}] directories with no media files...");
            foreach (var directory in directoriesWithoutMediaFiles.Distinct())
            {
                if (directory.DoesDirectoryHaveImageFiles())
                {
                    if (directory.Parent()?.DoesDirectoryHaveMediaFiles() ?? false)
                    {
                        var parentDir = directory.Parent()!;
                        if (parentDir.FullName().ToNormalizedString() != libDir.FullName().ToNormalizedString())
                        {
                            foreach (var imageFile in directory.AllFileImageTypeFileInfos())
                            {
                                string? newImageFileName = null;
                                if (ImageHelper.IsAlbumImage(imageFile) || ImageHelper.IsAlbumSecondaryImage(imageFile))
                                {
                                    newImageFileName = parentDir.GetNextFileNameForType(maxNumberOfAlbumImagesAllowed, ImageHelper.IsAlbumImage(imageFile) ? PictureIdentifier.Front.ToString() : PictureIdentifier.SecondaryFront.ToString()).Item1;
                                }
                                else if (ImageHelper.IsArtistImage(imageFile) || ImageHelper.IsArtistSecondaryImage(imageFile))
                                {
                                    newImageFileName = parentDir.GetNextFileNameForType(maxNumberOfArtistImagesAllowed, ImageHelper.IsArtistImage(imageFile) ? PictureIdentifier.Artist.ToString() : PictureIdentifier.ArtistSecondary.ToString()).Item1;
                                }

                                if (newImageFileName != null)
                                {
                                    File.Move(imageFile.FullName, newImageFileName);
                                    messages.Add($"Moved image file from [{imageFile.FullName}] to [{newImageFileName}]");
                                }
                            }
                        }
                    }
                }

                if (directory.Exists())
                {
                    directory.Delete();
                    messages.Add($"Directory [{directory}] deleted.");
                }
            }
        }

        allDirectoriesInLibrary = libDir.AllDirectoryInfos(searchOption: SearchOption.AllDirectories).ToArray();
        var libraryDirectoryCountAfterCleaning = allDirectoriesInLibrary.Length;
        var numberDeleted = libraryDirectoryCountBeforeCleaning - libraryDirectoryCountAfterCleaning;
        if (numberDeleted > 0)
        {
            messages.Add($"Deleted [{numberDeleted.ToStringPadLeft(DisplayNumberPadLength)}] directories from library. Library now has [{libraryDirectoryCountAfterCleaning.ToStringPadLeft(DisplayNumberPadLength)}] directories.");
        }

        var duplicateDirPrefix = configuration.GetValue<string>(SettingRegistry.ProcessingDuplicateAlbumPrefix);

        var melodeeFilesDeleted = 0;
        if (library.TypeValue != LibraryType.Inbound)
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                foreach (var directory in allDirectoriesInLibrary)
                {
                    if (duplicateDirPrefix != null)
                    {
                        if (directory.Name.StartsWith(duplicateDirPrefix))
                        {
                            continue;
                        }
                    }

                    var directoryInfo = directory.ToFileSystemDirectoryInfo();
                    if (directoryInfo != null && directoryInfo.AllMediaTypeFileInfos().Any())
                    {
                        var aPath = $"{directoryInfo.Name}/";
                        var dbAlbum = await scopedContext
                            .Albums.Include(x => x.Songs)
                            .FirstOrDefaultAsync(x => x.Directory == aPath, cancellationToken)
                            .ConfigureAwait(false);
                        if (dbAlbum == null)
                        {
                            messages.Add($"Unable to find album for directory [{directory}]");
                        }
                    }
                }
            }
        }


        //
        // var albumValidator = new AlbumValidator(configuration);
        // foreach (var melodeeJsonFile in Directory.GetFiles(library.Path, $"*{MelodeeModels.Album.JsonFileName}", SearchOption.AllDirectories))
        // {
        //     var fileInfo = new FileInfo(melodeeJsonFile);
        //     MelodeeModels.Album? album = null;
        //     try
        //     {
        //         album = _serializer.Deserialize<MelodeeModels.Album>(await File.ReadAllTextAsync(melodeeJsonFile, cancellationToken));
        //     }
        //     catch (Exception ex)
        //     {
        //         Logger.Warning(ex, "! Unable to load melodee album [{FileName}]", melodeeJsonFile);
        //         File.Delete(melodeeJsonFile);
        //         melodeeFilesDeleted++;
        //     }
        //     if (!albumValidator.ValidateAlbum(album).Data.IsValid)
        //     {
        //         if (skipDirPrefix != null)
        //         {
        //             if (fileInfo.Directory.Parent != null)
        //             {
        //                 var newName = Path.Combine(fileInfo.Directory.Parent.FullName, $"{skipDirPrefix}{fileInfo.Name}-{DateTime.UtcNow.Ticks}");
        //                 d.MoveTo(newName);
        //                 Logger.Warning("~ Moved invalid album directory [{Old}] to [{New}]", d.FullName, newName);
        //             }
        //         }
        //     
        //         continue;
        //     }        


        // if (library.TypeValue == LibraryType.Storage)
        // {
        //     // If album directory has media files, but does not have a melodee data file, and is not found in the db, then move back to inbound
        //     var inboundLibrary = await GetInboundLibraryAsync(cancellationToken).ConfigureAwait(false);
        //     d.MoveTo(Path.Combine(inboundLibrary.Data.Path, Guid.NewGuid().ToString()));
        //     result.Add(new MelodeeModels.Statistic(StatisticType.Warning, "~ Moved album directory", albumDirectory, StatisticColorRegistry.Warning, "Moved album folder to incoming"));
        //     continue;
        // }        

        // // If unable to load melodee data file then move back to Inbound
        // var inboundLibrary = await GetInboundLibraryAsync(cancellationToken).ConfigureAwait(false);
        // d.MoveTo(Path.Combine(inboundLibrary.Data.Path, Guid.NewGuid().ToString()));
        // result.Add(new MelodeeModels.Statistic(StatisticType.Warning,
        //     "~ Invalid Melodee file",
        //     albumDirectory,
        //     StatisticColorRegistry.Warning,
        //     "Invalid Melodee File. Deleted and moved directory to inbound library."));        

        // if (shouldResetLastScan)
        // {
        //     var dbLibrary = await scopedContext.Libraries.FirstAsync(x => x.Id == library.Id, cancellationToken).ConfigureAwait(false);
        //     dbLibrary.LastScanAt = null;
        //     await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        // }

        //  }


        if (library.TypeValue != LibraryType.Storage)
        {
            foreach (var melodeeJsonFile in Directory.GetFiles(library.Path, $"*{MelodeeModels.Album.JsonFileName}", SearchOption.AllDirectories))
            {
                File.Delete(melodeeJsonFile);
                melodeeFilesDeleted++;
            }

            if (melodeeFilesDeleted > 0)
            {
                messages.Add($"Deleted [{melodeeFilesDeleted}] melodee files from library.");
            }
        }

        return new MelodeeModels.OperationResult<string[]>
        {
            Data = messages.ToArray()
        };
    }


    // public static MelodeeModels.FileSystemDirectoryInfo[] GetDirectoriesWithoutMediaFiles(string directoryName)
    // {
    //     var result = new List<string>();
    //     var d = new DirectoryInfo(directoryName);
    //     foreach (var directory in d.EnumerateDirectories("*.*", SearchOption.AllDirectories))
    //     {
    //         if (!directory.DoesDirectoryHaveMediaFiles())
    //         {
    //             result.Add(directory.FullName);
    //         }
    //
    //         result.AddRange(GetDirectoriesWithoutMediaFiles(directory.FullName));
    //     }
    //
    //     if (!d.DoesDirectoryHaveMediaFiles())
    //     {
    //         result.Add(d.FullName);
    //     }
    //
    //     return result.Distinct().ToArray();
    // }

    public Task<MelodeeModels.OperationResult<bool>> DeleteAsync(int[] ids, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<MelodeeModels.OperationResult<bool>> UpdateAsync(Library library, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(library, nameof(library));

        var validationResult = ValidateModel(library);
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
                .Libraries
                .FirstOrDefaultAsync(x => x.Id == library.Id, cancellationToken)
                .ConfigureAwait(false);

            if (dbDetail == null)
            {
                return new MelodeeModels.OperationResult<bool>
                {
                    Data = false,
                    Type = MelodeeModels.OperationResponseType.NotFound
                };
            }

            dbDetail.Description = library.Description;
            dbDetail.IsLocked = library.IsLocked;
            dbDetail.LastUpdatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow);
            dbDetail.Name = library.Name;
            dbDetail.Notes = library.Notes;
            dbDetail.Path = library.Path;
            dbDetail.Notes = library.Notes;
            dbDetail.SortOrder = library.SortOrder;
            dbDetail.Type = library.Type;
            dbDetail.Tags = library.Tags;

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
}
