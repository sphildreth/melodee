using System.Collections.Concurrent;
using Ardalis.GuardClauses;
using Dapper;
using IdSharp.Common.Utils;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.MessageBus;
using Melodee.Common.MessageBus.Events;
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
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NodaTime;
using Serilog;
using ServiceStack;
using SixLabors.ImageSharp;
using SmartFormat;
using MelodeeModels = Melodee.Common.Models;

namespace Melodee.Services;

public sealed class LibraryService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    ISettingService settingService,
    ISerializer serializer,
    IEventPublisher<AlbumUpdatedEvent>? albumUpdatedEvent)
    : ServiceBase(logger, cacheManager, contextFactory), ILibraryService
{
    private const string CacheKeyDetailByApiKeyTemplate = "urn:library:apikey:{0}";
    private const string CacheKeyDetailLibraryByType = "urn:library_by_type:{0}";
    private const string CacheKeyDetailTemplate = "urn:library:{0}";

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

    public async Task ProcessExistingDirectoryMoveMergeAsync(MelodeeModels.Album albumToMove, string existingAlbumPath, CancellationToken cancellationToken = default)
    {
        var modifiedExistingDirectory = false;
        
        var albumToMoveDir = albumToMove.Directory;
        var existingDir = new DirectoryInfo(existingAlbumPath);
        
        Logger.Debug("[{ServiceName}] :\u2552: processing existing directory merge from [{ExistingDirectoryPath}] to [{NewDirectoryPath}]", nameof(LibraryService),
            albumToMove.Directory.Name, existingDir.Name);
        
        if (albumToMove.Images?.Any() == true)
        {
            var existingImages = ImageHelper.ImageFilesInDirectory(existingDir.FullName, SearchOption.TopDirectoryOnly).ToList();
            var existingImagesCrc = existingImages.Select(async x => new { Crc = CRC32.Calculate(await File.ReadAllBytesAsync(x, cancellationToken)), ImageFileName = x }).ToArray();
            var imagesToMoveCrc = albumToMove.Images.Select(x => new { Crc = x.CrcHash, ImageFileName = x.FileInfo!.FullName(albumToMoveDir) }).ToList();
            
            // if none exist take all images from albumToMove
            if (existingImages.Count == 0)
            {
                Console.WriteLine("No image found in existing directory. Copying all images from Album...");
                foreach (var image in albumToMove.Images)
                {
                    File.Move(image.FileInfo.FullName(albumToMoveDir), Path.Combine(existingDir.FullName, image.FileInfo.Name));
                    Logger.Debug("[{ServiceName}] :\u2502: moving new image [{FileName}]", nameof(LibraryService), image.FileInfo.Name);
                    modifiedExistingDirectory = true;
                }
            }
            else
            {
                var existingImageInfos = await Task.WhenAll(existingImagesCrc.Select(x => x)).ConfigureAwait(false);
                foreach (var imageToMove in imagesToMoveCrc.ToArray())
                {
                    if (existingImageInfos.Any(x => x.Crc == imageToMove.Crc))
                    {
                        // Exact duplicate found, dont do anything
                        imagesToMoveCrc.Remove(imageToMove);
                        continue;
                    }
                    var existingWithSameFileNames = existingImageInfos.Where(x => string.Equals(x.ImageFileName, imageToMove.ImageFileName, StringComparison.OrdinalIgnoreCase)).ToArray(); 
                    foreach(var existingWithSameFileName  in existingWithSameFileNames)
                    {
                        imagesToMoveCrc.Remove(imageToMove);
                        
                        // If some exist, not duplicate CRC, same name, keep the higher resolution
                        var existingInfoSizeInfo = await Image.IdentifyAsync(existingWithSameFileName.ImageFileName, cancellationToken).ConfigureAwait(false);
                        var toMoveInfoSizeInfo = await Image.IdentifyAsync(imageToMove.ImageFileName, cancellationToken).ConfigureAwait(false);
                        if (existingInfoSizeInfo.Width > toMoveInfoSizeInfo.Width)
                        {
                            continue;
                        }

                        var toFile = Path.Combine(existingDir.FullName, Path.GetFileName(imageToMove.ImageFileName));
                        Console.WriteLine($"Existing image [{existingWithSameFileName.ImageFileName}] to be overwritten by image [{toFile}]...");
                        File.Delete(existingWithSameFileName.ImageFileName);
                        File.Move(imageToMove.ImageFileName, toFile);
                        Logger.Debug("[{ServiceName}] :\u2502: moving better image [{FileName}]", nameof(LibraryService), Path.GetFileName(imageToMove.ImageFileName));
                        modifiedExistingDirectory = true;
                    }
                }
                foreach (var imageToMove in imagesToMoveCrc)
                {
                    var toFile = Path.Combine(existingDir.FullName, Path.GetFileName(imageToMove.ImageFileName));
                    if (!File.Exists(toFile))
                    {
                        Console.WriteLine($"Moving image [{imageToMove.ImageFileName}] to [{toFile}]...");
                        File.Move(imageToMove.ImageFileName, toFile);
                        Logger.Debug("[{ServiceName}] :\u2502: moving image [{FileName}]", nameof(LibraryService), Path.GetFileName(imageToMove.ImageFileName));
                        modifiedExistingDirectory = true;
                    }
                }
            }
        }

        var songsToMove = albumToMove.Songs?.ToList() ?? [];
        if (songsToMove.Any())
        {
            var configuration = await settingService.GetMelodeeConfigurationAsync(cancellationToken);
            var imageValidator = new ImageValidator(configuration);
            var imageConvertor = new ImageConvertor(configuration);
            var atlMetTag = new AtlMetaTag(new MetaTagsProcessor(configuration, serializer), imageConvertor, imageValidator, configuration);
            var existingSongsFileInfos = albumToMoveDir.AllMediaTypeFileInfos().ToArray();
            var existingSongs = new List<Common.Models.Song>();
            foreach (var song in existingSongsFileInfos)
            {
                var songLoadResult = await atlMetTag.ProcessFileAsync(existingDir.ToDirectorySystemInfo(), song.ToFileSystemInfo(), cancellationToken).ConfigureAwait(false);
                if (songLoadResult.IsSuccess)
                {
                    existingSongs.Add(songLoadResult.Data);
                }
            }
            foreach (var songToMove in songsToMove.ToArray())
            {
                var existingForSongToMove = existingSongs.FirstOrDefault(x => x.MediaNumber() == songToMove.MediaNumber() && 
                                                                                   x.SongNumber() == songToMove.SongNumber());
                if (existingForSongToMove != null)
                {
                    // TODO for some reason the song.CrcHash is wrong? 
                    var songToMoveCrcHash = Crc32.Calculate(songToMove.File.ToFileInfo(albumToMoveDir));
                    if (existingForSongToMove.CrcHash == songToMoveCrcHash)
                    {
                        // Duplicate dont do anything
                        songsToMove.Remove(songToMove);
                        continue;
                    }
                    if (existingForSongToMove.File.Size > songToMove.File.Size || 
                        (existingForSongToMove.BitRate() > songToMove.BitRate() && existingForSongToMove.BitDepth() > songToMove.BitDepth()))
                    {
                        // existing song is better don't do anything
                        songsToMove.Remove(songToMove);
                    }
                }
            }
            // Copy over any song left to move
            foreach (var songToMove in songsToMove)
            {
                var toFile = Path.Combine(existingDir.FullName, songToMove.File.Name);
                if (!File.Exists(toFile))
                {
                    Logger.Debug("[{ServiceName}] :\u2502: moving song [{FileName}]", nameof(LibraryService), songToMove.File.Name);
                    File.Move(songToMove.File.FullName(albumToMoveDir), toFile);                    
                    modifiedExistingDirectory = true;
                }
            }            
        }
        // Delete directory to merge as what is wanted has been moved 
        Directory.Delete(albumToMove.Directory.FullName(), true);
        Logger.Debug("[{ServiceName}] :\u2558: deleting directory [{FileName}]", nameof(LibraryService), albumToMove.Directory);
        if (modifiedExistingDirectory && albumUpdatedEvent != null)
        {
            await albumUpdatedEvent.Publish(new Event<AlbumUpdatedEvent>(new AlbumUpdatedEvent(null, existingAlbumPath)), cancellationToken).ConfigureAwait(false);
        }
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
                await ProcessExistingDirectoryMoveMergeAsync(album, libraryAlbumPath, cancellationToken).ConfigureAwait(false);
                continue;
            }

            var libraryArtistDirectoryInfo = new DirectoryInfo(Path.Combine(library.Path, artistDirectory)).ToDirectorySystemInfo();
            var libraryAlbumDirectoryInfo = new DirectoryInfo(libraryAlbumPath).ToDirectorySystemInfo();
            MediaEditService.MoveDirectory(album.Directory.FullName(), libraryAlbumPath);
            var melodeeFileName = Path.Combine(libraryAlbumPath, "melodee.json");
            var melodeeFile = serializer.Deserialize<MelodeeModels.Album>(await File.ReadAllTextAsync(melodeeFileName, cancellationToken));
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
                    var existingArtistImagesCrc32s = existingArtistImages.Select(Crc32.Calculate).ToArray();
                    foreach (var image in album.Artist.Images.ToArray())
                    {
                        if (image.FileInfo != null)
                        {
                            // If there are artist images, check CRC and see if duplicate, delete any duplicate found in album directory
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
                        var newName = Path.Combine(dirInfo.Parent.FullName, $"{skipDirPrefix}{dirInfo.Name}-{DateTime.UtcNow.Ticks}");
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

    public async Task<MelodeeModels.OperationResult<MelodeeModels.Statistic[]?>> AlbumStatusReport(string libraryName, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrEmpty(libraryName, nameof(libraryName));
        
        var libraries = await ListAsync(new MelodeeModels.PagedRequest { PageSize = short.MaxValue }, cancellationToken).ConfigureAwait(false);
        var library = libraries.Data.FirstOrDefault(x => x.Name.ToNormalizedString() == libraryName.ToNormalizedString());
        if (library == null)
        {
            return new MelodeeModels.OperationResult<MelodeeModels.Statistic[]?>("Invalid From library Name")
            {
                Data = []
            };
        }
        
        var result = new List<MelodeeModels.Statistic>();
        
        // Get all melodee albums in library path
        var libraryDirectory = new DirectoryInfo(library.Path);
        var melodeeFileSystemInfosForLibrary = libraryDirectory.GetFileSystemInfos(Common.Models.Album.JsonFileName, SearchOption.AllDirectories).ToArray();
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
            var melodeeFile = serializer.Deserialize<MelodeeModels.Album>(await File.ReadAllBytesAsync(melodeeFileSystemInfo.FullName, cancellationToken));
            if (melodeeFile != null)
            {
                melodeeFilesForLibrary.Add(melodeeFile);
            }
        }

        var melodeeFilesGrouped = melodeeFilesForLibrary.GroupBy(x => x.Status);
        var melodeeFilesGroupedOk = melodeeFilesGrouped.FirstOrDefault(x => x.Key == AlbumStatus.Ok);
        if (melodeeFilesGroupedOk != null)
        {
            result.Add(new MelodeeModels.Statistic(StatisticType.Information, melodeeFilesGroupedOk.Key.ToString(), melodeeFilesGroupedOk.Count(), StatisticColorRegistry.Ok, "These albums are ok!"));
        }

        foreach (var album in melodeeFilesForLibrary.Where(x => x.Status != AlbumStatus.Ok))
        {
            result.Add(new MelodeeModels.Statistic(StatisticType.Warning, album.Directory.Name, album.StatusReasons.ToString(), StatisticColorRegistry.Warning));            
        }
        
        return new MelodeeModels.OperationResult<MelodeeModels.Statistic[]?>()
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

        var shouldResetLastScan = false;
        
        var result = new List<MelodeeModels.Statistic>
        {
            new MelodeeModels.Statistic(StatisticType.Information, "Artist Count", library.ArtistCount.ToStringPadLeft(DisplayNumberPadLength) ?? "0", StatisticColorRegistry.Ok, "Number of artists on Library db record."),
            new MelodeeModels.Statistic(StatisticType.Information, "Album Count", library.AlbumCount.ToStringPadLeft(DisplayNumberPadLength) ?? "0", StatisticColorRegistry.Ok, "Number of albums on Library db record."),
            new MelodeeModels.Statistic(StatisticType.Information,"Song Count", library.SongCount.ToStringPadLeft(DisplayNumberPadLength) ?? "0", StatisticColorRegistry.Ok,"Number of songs on Library db record.")
        };

        if (library.TypeValue == LibraryType.Library)
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
                                    var dbArtist = await scopedContext.Artists.FirstOrDefaultAsync(x => x.Directory == dPath, cancellationToken: cancellationToken).ConfigureAwait(false);
                                    if (dbArtist == null)
                                    {
                                        result.Add(new MelodeeModels.Statistic(StatisticType.Error,"! Unknown artist directory", artistDirectory, StatisticColorRegistry.Error, $"Unable to find artist for directory [{d.Name}]"));
                                        shouldResetLastScan = true;
                                    }

                                    artistDirectoriesFound++;
                                    foreach (var albumDirectory in Directory.GetDirectories(artistDirectory, "*", SearchOption.TopDirectoryOnly))
                                    {
                                        d = new DirectoryInfo(albumDirectory);
                                        var aPath = $"{d.Name}/";
                                        var dbAlbum = await scopedContext
                                            .Albums.Include(x => x.Discs).ThenInclude(x => x.Songs)
                                            .FirstOrDefaultAsync(x => x.Directory == aPath, cancellationToken: cancellationToken)
                                            .ConfigureAwait(false);
                                        if (dbAlbum == null)
                                        {
                                            if (library.TypeValue == LibraryType.Library)
                                            {
                                                // If album directory has media files, but does not have a melodee data file, and is not found in the db, then move back to inbound
                                                var melodeeFile = d.GetFileSystemInfos(Melodee.Common.Models.Album.JsonFileName, SearchOption.TopDirectoryOnly).FirstOrDefault();
                                                if (melodeeFile == null)
                                                {
                                                    var inboundLibrary = await GetInboundLibraryAsync(cancellationToken).ConfigureAwait(false);
                                                    d.MoveTo(Path.Combine(inboundLibrary.Data.Path, Guid.NewGuid().ToString()));
                                                    result.Add(new MelodeeModels.Statistic(StatisticType.Warning, "~ Moved album directory", albumDirectory, StatisticColorRegistry.Warning, "Moved album folder to incoming"));
                                                    continue;
                                                }
                                            }
                                            result.Add(new MelodeeModels.Statistic(StatisticType.Error,"! Unknown album directory", albumDirectory, StatisticColorRegistry.Error, $"Unable to find album for directory [{d.Name}]"));
                                            shouldResetLastScan = true;
                                        }
                                        var albumSongsFound = 0;
                                        foreach (var songFound in d.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly).Where(x => FileHelper.IsFileMediaType(x.Extension)).OrderBy(x => x.Name))
                                        {
                                            var dbSong = dbAlbum?.Discs.SelectMany(x => x.Songs).FirstOrDefault(x => x.FileName == songFound.Name);
                                            if (dbSong == null)
                                            {
                                                result.Add(new MelodeeModels.Statistic(StatisticType.Error,"! Unknown song", songFound.Name, StatisticColorRegistry.Error, $"Album Id [{dbAlbum?.Id}]: Unable to find song for album"));
                                                shouldResetLastScan = true;
                                            }
                                            albumSongsFound++;
                                        }

                                        if (albumSongsFound != dbAlbum?.SongCount)
                                        {
                                            result.Add(new MelodeeModels.Statistic(StatisticType.Error,"! Album song count mismatch ", $"{dbArtist?.Directory}{d.Name}", StatisticColorRegistry.Error, $"Album Id [{dbAlbum?.Id}]: Found [{albumSongsFound.ToStringPadLeft(4)}] album has [{dbAlbum?.SongCount.ToStringPadLeft(4)}]"));
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
                    result.Add(new MelodeeModels.Statistic(StatisticType.Error,"! Album directory missing Melodee data file",albumDirectoriesWithoutMelodeeDataFile, StatisticColorRegistry.Error, $"When scanning media without a Melodee data file, media files will not get processed."));
                }

                shouldResetLastScan = shouldResetLastScan && library.TypeValue == LibraryType.Library;

                if (shouldResetLastScan)
                {
                    var dbLibrary = await scopedContext.Libraries.FirstAsync(x => x.Id == library.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
                    dbLibrary.LastScanAt = null;
                    await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }

                var message = artistDirectoriesFound == library.ArtistCount ? null : $"Artist directory count does not match Library artist count.";
                result.Add(new MelodeeModels.Statistic(artistDirectoriesFound == library.ArtistCount ? StatisticType.Information : StatisticType.Warning,"Artist Directories Found", artistDirectoriesFound.ToStringPadLeft(DisplayNumberPadLength), artistDirectoriesFound == library.ArtistCount ? StatisticColorRegistry.Ok : StatisticColorRegistry.Warning, message));

                message = albumDirectoriesFound == library.AlbumCount ? null : $"Album directory count does not match Library album count.";
                result.Add(new MelodeeModels.Statistic(albumDirectoriesFound == library.AlbumCount ? StatisticType.Information : StatisticType.Warning,"Album Directories Found", albumDirectoriesFound.ToStringPadLeft(DisplayNumberPadLength), albumDirectoriesFound == library.AlbumCount ? StatisticColorRegistry.Ok : StatisticColorRegistry.Warning, message));

                message = songsFound == library.SongCount ? null : $"Song count [{songsFound.ToStringPadLeft(DisplayNumberPadLength)}] does not match Library song count [{library.SongCount.ToStringPadLeft(DisplayNumberPadLength)}].";
                result.Add(new MelodeeModels.Statistic(songsFound == library.SongCount ? StatisticType.Information :StatisticType.Error,"Songs Found", songsFound.ToStringPadLeft(DisplayNumberPadLength), songsFound == library.SongCount ? StatisticColorRegistry.Ok : StatisticColorRegistry.Warning, message));
            }
        }

        return new MelodeeModels.OperationResult<MelodeeModels.Statistic[]?>(shouldResetLastScan ? "You should run `library scan`" : string.Empty)
        {
            Data = result.ToArray()
        };
    }

    public async Task<MelodeeModels.OperationResult<string[]>> CleanLibraryAsync(string settingsLibraryName, CancellationToken cancellationToken = default)
    {
        var result = false;
        
        Guard.Against.NullOrEmpty(settingsLibraryName, nameof(settingsLibraryName));

        var libraries = await ListAsync(new MelodeeModels.PagedRequest { PageSize = short.MaxValue }, cancellationToken).ConfigureAwait(false);
        var library = libraries.Data.FirstOrDefault(x => x.Name.ToNormalizedString() == settingsLibraryName.ToNormalizedString());
        if (library == null)
        {
            return new MelodeeModels.OperationResult<string[]>("Invalid From library Name")
            {
                Data = []
            };
        }
        var messages = new List<string>();
        Console.WriteLine($"Cleaning [{library.Path}]...");
        var allDirectoriesInLibrary = Directory.GetDirectories(library.Path, "*.*", SearchOption.TopDirectoryOnly).ToArray();
        Console.WriteLine($"Found [{allDirectoriesInLibrary.Length}] directories...");
        var libraryDirectoryCountBeforeCleaning = allDirectoriesInLibrary.Length;
        var directoriesWithoutMediaFiles = new ConcurrentBag<string>();
        Parallel.ForEach(allDirectoriesInLibrary, directory =>
        {
            GetDirectoriesWithoutMediaFiles(directory).ForEach((s, i) => directoriesWithoutMediaFiles.Add(s) );
        });
        if (directoriesWithoutMediaFiles.Distinct().Any())
        {
            Console.WriteLine($"Found [{directoriesWithoutMediaFiles.Count}] directories with no media files...");
            foreach (var directory in directoriesWithoutMediaFiles.Distinct())
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                    messages.Add($"Directory [{directory}] deleted.");
                }
            }
        }
        allDirectoriesInLibrary = Directory.GetDirectories(library.Path, "*", SearchOption.AllDirectories).ToArray();
        var libraryDirectoryCountAfterCleaning = allDirectoriesInLibrary.Length;
        messages.Add($"Deleted [{libraryDirectoryCountBeforeCleaning-libraryDirectoryCountAfterCleaning}] directories from library. Library now has [{libraryDirectoryCountAfterCleaning.ToStringPadLeft(DisplayNumberPadLength)}] directories.");
        return new MelodeeModels.OperationResult<string[]>
        {
            Data = messages.ToArray(),
        };
    }

    public static string[] GetDirectoriesWithoutMediaFiles(string directoryName)
    {
        var result = new List<string>();
        var d = new DirectoryInfo(directoryName);
        foreach (var directory in d.EnumerateDirectories("*.*", SearchOption.AllDirectories))
        {
            if (!directory.DoesDirectoryHaveMediaFiles())
            {
                result.Add(directory.FullName);
            }
            result.AddRange(GetDirectoriesWithoutMediaFiles(directory.FullName));
        }

        if (!d.DoesDirectoryHaveMediaFiles())
        {
            result.Add(d.FullName);
        }
        return result.Distinct().ToArray();
    }
}
