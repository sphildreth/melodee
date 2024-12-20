using Ardalis.GuardClauses;
using Dapper;
using IdSharp.Common.Utils;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Serialization;
using Melodee.Common.Utility;
using Melodee.Plugins.Conversion.Image;
using Melodee.Plugins.MetaData.Song;
using Melodee.Plugins.Processor;
using Melodee.Plugins.Validation;
using Melodee.Services.Interfaces;
using Melodee.Services.Models;
using Melodee.Services.Scanning;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;
using SmartFormat;
using Log = ATL.Logging.Log;
using MelodeeModels = Melodee.Common.Models;

namespace Melodee.Services;

public sealed class LibraryService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    ISettingService settingService,
    ISerializer serializer)
    : ServiceBase(logger, cacheManager, contextFactory), ILibraryService
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
                    Type = MelodeeModels.OperationResponseType.Error
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

    public async Task<MelodeeModels.OperationResult<bool>> MoveAlbumsToLibrary(Library library, MelodeeModels.Album[] albums, CancellationToken cancellationToken = default)
    {
        var configuration = await settingService.GetMelodeeConfigurationAsync(cancellationToken);

        if (albums.Any(x => !x.IsValid(configuration.Configuration).Item1))
        {
            return new MelodeeModels.OperationResult<bool>(albums.Where(x => !x.IsValid(configuration.Configuration).Item1).Select(x => $"Album [{x}] is invalid."))
            {
                Data = false
            };
        }

        var maxArtistImageCount = configuration.GetValue<short>(SettingRegistry.ImagingMaximumNumberOfArtistImages);
        var movedCount = 0;
        foreach (var album in albums)
        {
            var artistDirectory = album.Artist.ToDirectoryName(configuration.GetValue<short>(SettingRegistry.ProcessingMaximumArtistDirectoryNameLength));
            var albumDirectory = album.AlbumDirectoryName(configuration.Configuration);
            var libraryAlbumPath = Path.Combine(library.Path, artistDirectory, albumDirectory);
            if (!Directory.Exists(libraryAlbumPath))
            {
                Directory.CreateDirectory(libraryAlbumPath);
            }
            else
            {
                //TODO if data album exists for model album if so determine which is better quality
                Logger.Warning("Existing directory found. Skipping album moving.");
                continue;
            }

            var libraryArtistDirectoryInfo = new DirectoryInfo(Path.Combine(library.Path, artistDirectory)).ToDirectorySystemInfo();
            var libraryAlbumDirectoryInfo = new DirectoryInfo(libraryAlbumPath).ToDirectorySystemInfo();
            MediaEditService.MoveDirectory(album.Directory!.FullName(), libraryAlbumPath);
            var melodeeFileName = Path.Combine(libraryAlbumPath, "melodee.json");
            var melodeeFile = serializer.Deserialize<MelodeeModels.Album>(await File.ReadAllTextAsync(melodeeFileName, cancellationToken));
            melodeeFile!.Directory!.Path = libraryAlbumPath;
            if (album.Artist.Images?.Any() ?? false)
            {
                var existingArtistImages = libraryArtistDirectoryInfo.AllFileImageTypeFileInfos().Where(x => ImageHelper.IsArtistImage(x) || ImageHelper.IsArtistSecondaryImage(x)).ToArray();
                if (existingArtistImages.Length == 0)
                {
                    // If there are no artist images in artists library directory, move artist images from album folder
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
                    var existingArtistImagesCrc32s = existingArtistImages.Select(Crc32.Calculate).ToArray();
                    foreach (var image in album.Artist.Images.ToArray())
                    {
                        if (image.FileInfo != null)
                        {
                            // If there are artist images, check CRC and see if duplicate, delete any duplicate found in album folder
                            if (existingArtistImagesCrc32s.Contains(CRC32.Calculate(image.FileInfo.ToFileInfo(libraryArtistDirectoryInfo))))
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

            ClearCache();
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

    private void ClearCache()
    {
        CacheManager.Remove(CacheKeyDetailLibraryByType.FormatSmart((int)LibraryType.Inbound));
        CacheManager.Remove(CacheKeyDetailLibraryByType.FormatSmart((int)LibraryType.Library));
        CacheManager.Remove(CacheKeyDetailLibraryByType.FormatSmart((int)LibraryType.Staging));
    }

    public async Task<MelodeeModels.OperationResult<bool>> MoveAlbumsFromLibraryToLibrary(string fromLibraryName, string toLibraryName, Func<MelodeeModels.Album, bool> condition, bool verboseSet, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrEmpty(fromLibraryName, nameof(fromLibraryName));
        Guard.Against.NullOrEmpty(fromLibraryName, nameof(toLibraryName));

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

        if (toLibrary.TypeValue != LibraryType.Library)
        {
            return new MelodeeModels.OperationResult<bool>($"Invalid library type, this move process requires a library type of 'Library' ({(int)LibraryType.Library}).")
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

        var configuration = await settingService.GetMelodeeConfigurationAsync(cancellationToken);

        var imageValidator = new ImageValidator(configuration);
        var imageConvertor = new ImageConvertor(configuration);

        ISongPlugin[] songPlugins =
        [
            new AtlMetaTag(new MetaTagsProcessor(configuration, serializer), imageConvertor,  imageValidator, configuration)
        ];
        var skipDirPrefix = configuration.GetValue<string>(SettingRegistry.ProcessingSkippedDirectoryPrefix).Nullify(); 
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
            if (skipDirPrefix != null)
            {
                if (dirInfo.Name.StartsWith(skipDirPrefix))
                {
                    continue;
                }
            }            
            var album = serializer.Deserialize<MelodeeModels.Album>(await File.ReadAllBytesAsync(albumFile, cancellationToken));
            if (album != null)
            {
                if (!album.IsValid(configuration.Configuration).Item1)
                {
                    if (skipDirPrefix != null)
                    {
                        var newName = Path.Combine(dirInfo.Parent.FullName, $"{skipDirPrefix}{dirInfo.Name}");
                        dirInfo.MoveTo(newName);
                        Logger.Warning("Moved invalid album directory [{Old}] to [{New}]", dirInfo.FullName, newName);
                    }
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
}
