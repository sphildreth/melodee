using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using IdSharp.Common.Utils;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Data.Models.DTOs;
using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Models.OpenSubsonic;
using Melodee.Common.Plugins.Conversion.Image;
using Melodee.Common.Plugins.MetaData.Song;
using Melodee.Common.Plugins.Processor;
using Melodee.Common.Plugins.Validation;
using Melodee.Common.Serialization;
using Melodee.Common.Services.Caching;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;
using Serilog.Events;
using SerilogTimings;
using SixLabors.ImageSharp;
using Artist = Melodee.Common.Models.Artist;
using Directory = System.IO.Directory;

namespace Melodee.Common.Services;

public abstract class ServiceBase
{
    public const string CacheName = "melodee";

    protected static TimeSpan DefaultCacheDuration = TimeSpan.FromDays(1);

    /// <summary>
    ///     This is required for Mocking in unit tests.
    /// </summary>
    protected ServiceBase()
    {
    }

    protected ServiceBase(
        ILogger logger,
        ICacheManager cacheManager,
        IDbContextFactory<MelodeeDbContext> contextFactory)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        CacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
        ContextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
    }

    protected ILogger Logger { get; } = null!;
    protected ICacheManager CacheManager { get; } = null!;
    protected IDbContextFactory<MelodeeDbContext> ContextFactory { get; } = null!;

    protected async Task ProcessExistingDirectoryMoveMergeAsync(IMelodeeConfiguration configuration,
        ISerializer serializer, Album albumToMove, string existingAlbumPath,
        CancellationToken cancellationToken = default)
    {
        var albumToMoveDir = albumToMove.Directory;
        var existingDir = new DirectoryInfo(existingAlbumPath);

        Logger.Debug(
            "[{ServiceName}] :\u2552: processing existing directory merge from [{ExistingDirectoryPath}] to [{NewDirectoryPath}]",
            nameof(LibraryService),
            albumToMove.Directory.Name, existingDir.Name);

        if (albumToMove.Images?.Any() == true)
        {
            var existingImages = ImageHelper.ImageFilesInDirectory(existingDir.FullName, SearchOption.TopDirectoryOnly)
                .ToList();
            var existingImagesCrc = existingImages.Select(async x => new
                    { Crc = CRC32.Calculate(await File.ReadAllBytesAsync(x, cancellationToken)), ImageFileName = x })
                .ToArray();
            var imagesToMoveCrc = albumToMove.Images
                .Select(x => new { Crc = x.CrcHash, ImageFileName = x.FileInfo!.FullName(albumToMoveDir) }).ToList();

            // if none exist take all images from albumToMove
            if (existingImages.Count == 0)
            {
                Trace.WriteLine("No image found in existing directory. Copying all images from Album...");
                foreach (var image in albumToMove.Images)
                {
                    if (image.FileInfo != null)
                    {
                        var imagePath = image.FileInfo.FullName(albumToMoveDir);
                        if (File.Exists(imagePath))
                        {
                            File.Move(imagePath, Path.Combine(existingDir.FullName, image.FileInfo.Name));
                            Logger.Debug("[{ServiceName}] :\u2502: moving new image [{FileName}]",
                                nameof(LibraryService), image.FileInfo.Name);
                        }
                    }
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

                    var existingWithSameFileNames = existingImageInfos.Where(x =>
                            string.Equals(x.ImageFileName, imageToMove.ImageFileName,
                                StringComparison.OrdinalIgnoreCase))
                        .ToArray();
                    foreach (var existingWithSameFileName in existingWithSameFileNames)
                    {
                        imagesToMoveCrc.Remove(imageToMove);

                        // If some exist, not duplicate CRC, same name, keep the higher resolution
                        var existingInfoSizeInfo = await Image
                            .IdentifyAsync(existingWithSameFileName.ImageFileName, cancellationToken)
                            .ConfigureAwait(false);
                        var toMoveInfoSizeInfo = await Image.IdentifyAsync(imageToMove.ImageFileName, cancellationToken)
                            .ConfigureAwait(false);
                        if (existingInfoSizeInfo.Width > toMoveInfoSizeInfo.Width)
                        {
                            continue;
                        }

                        var toFile = Path.Combine(existingDir.FullName, Path.GetFileName(imageToMove.ImageFileName));
                        Trace.WriteLine(
                            $"Existing image [{existingWithSameFileName.ImageFileName}] to be overwritten by image [{toFile}]...");
                        File.Delete(existingWithSameFileName.ImageFileName);
                        File.Move(imageToMove.ImageFileName, toFile);
                        Logger.Debug("[{ServiceName}] :\u2502: moving better image [{FileName}]",
                            nameof(LibraryService), Path.GetFileName(imageToMove.ImageFileName));
                    }
                }

                foreach (var imageToMove in imagesToMoveCrc)
                {
                    var toFile = Path.Combine(existingDir.FullName, Path.GetFileName(imageToMove.ImageFileName));
                    if (!File.Exists(toFile))
                    {
                        Trace.WriteLine($"Moving image [{imageToMove.ImageFileName}] to [{toFile}]...");
                        File.Move(imageToMove.ImageFileName, toFile);
                        Logger.Debug("[{ServiceName}] :\u2502: moving image [{FileName}]", nameof(LibraryService),
                            Path.GetFileName(imageToMove.ImageFileName));
                    }
                }
            }
        }

        var songsToMove = albumToMove.Songs?.ToList() ?? [];
        if (songsToMove.Any())
        {
            var imageValidator = new ImageValidator(configuration);
            var imageConvertor = new ImageConvertor(configuration);
            var atlMetTag = new AtlMetaTag(new MetaTagsProcessor(configuration, serializer), imageConvertor,
                imageValidator, configuration);
            var existingSongsFileInfos = albumToMoveDir.AllMediaTypeFileInfos().ToArray();
            var existingSongs = new List<Song>();
            foreach (var song in existingSongsFileInfos)
            {
                var songLoadResult = await atlMetTag
                    .ProcessFileAsync(existingDir.ToDirectorySystemInfo(), song.ToFileSystemInfo(), cancellationToken)
                    .ConfigureAwait(false);
                if (songLoadResult.IsSuccess)
                {
                    existingSongs.Add(songLoadResult.Data);
                }
            }

            foreach (var songToMove in songsToMove.ToArray())
            {
                var existingForSongToMove =
                    existingSongs.FirstOrDefault(x => x.SongNumber() == songToMove.SongNumber());
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
                        (existingForSongToMove.BitRate() > songToMove.BitRate() &&
                         existingForSongToMove.BitDepth() > songToMove.BitDepth()))
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
                if (File.Exists(songToMove.File.FullName(albumToMoveDir)) && !File.Exists(toFile))
                {
                    Logger.Debug("[{ServiceName}] :\u2502: moving song [{FileName}]", nameof(LibraryService),
                        songToMove.File.Name);
                    File.Move(songToMove.File.FullName(albumToMoveDir), toFile);
                }
            }
        }

        // Delete directory to merge as what is wanted has been moved 
        Directory.Delete(albumToMove.Directory.FullName(), true);
        Logger.Debug("[{ServiceName}] :\u2558: deleting directory [{FileName}]", nameof(LibraryService), albumToMove.Directory);

    }


    protected async Task<OperationResult<bool>> UpdateArtistAggregateValuesByIdAsync(int artistId,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        
        var artist = await scopedContext.Artists
            .Where(a => a.Id == artistId)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (artist == null)
        {
            return new OperationResult<bool> { Data = false };
        }

        var albumCount = await scopedContext.Albums
            .Where(a => a.ArtistId == artistId)
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        var songCount = await scopedContext.Songs
            .Where(s => s.Album.ArtistId == artistId)
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        var updated = false;
        if (artist.AlbumCount != albumCount)
        {
            artist.AlbumCount = albumCount;
            artist.LastUpdatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow);
            updated = true;
        }

        if (artist.SongCount != songCount)
        {
            artist.SongCount = songCount;
            artist.LastUpdatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow);
            updated = true;
        }

        if (updated)
        {
            await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return new OperationResult<bool> { Data = updated };
    }


    protected async Task<OperationResult<bool>> UpdateLibraryAggregateStatsByIdAsync(int libraryId,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        
        var library = await scopedContext.Libraries
            .Where(l => l.Id == libraryId)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (library == null)
        {
            return new OperationResult<bool> { Data = false };
        }

        var artistCount = await scopedContext.Artists
            .Where(a => a.LibraryId == libraryId)
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        var albumCount = await scopedContext.Albums
            .Where(aa => aa.Artist.LibraryId == libraryId)
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        var songCount = await scopedContext.Songs
            .Where(s => s.Album.Artist.LibraryId == libraryId)
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        library.ArtistCount = artistCount;
        library.AlbumCount = albumCount;
        library.SongCount = songCount;
        library.LastUpdatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow);

        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new OperationResult<bool> { Data = true };
    }


    protected async Task<AlbumList2[]> AlbumListForArtistApiKey(Guid artistApiKey, int userId,
        CancellationToken cancellationToken)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        
        var query = from a in scopedContext.Albums
            join aa in scopedContext.Artists on a.ArtistId equals aa.Id
            join l in scopedContext.Libraries on aa.LibraryId equals l.Id
            where aa.ApiKey == artistApiKey
            select new { Album = a, Artist = aa, Library = l };

        var albumsWithArtists = await query
            .Include(x => x.Album.UserAlbums.Where(ua => ua.UserId == userId))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var albumList = new List<AlbumList2>();
        
        foreach (var item in albumsWithArtists)
        {
            var userAlbum = item.Album.UserAlbums.FirstOrDefault();

            albumList.Add(new AlbumList2
            {
                Id = $"album_{item.Album.ApiKey}",
                Album = item.Album.Name,
                Title = item.Album.Name,
                Name = item.Album.Name,
                CoverArt = $"album_{item.Album.ApiKey}",
                SongCount = item.Album.SongCount ?? 0,
                CreatedRaw = item.Album.CreatedAt,
                Duration = (int)(item.Album.Duration / 1000),
                PlayedCount = item.Album.PlayedCount,
                ArtistId = $"artist_{item.Artist.ApiKey}",
                Artist = item.Artist.Name,
                Year = item.Album.ReleaseDate.Year,
                Genres = item.Album.Genres,
                Parent = $"library_{item.Library.ApiKey}",
                UserRating = userAlbum?.Rating
            });
        }

        return albumList.ToArray();
    }

    protected async Task<DatabaseDirectoryInfo?> DatabaseArtistInfoForArtistApiKey(Guid apiKeyId, int userId,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        
        var result = await (from a in scopedContext.Artists
            join l in scopedContext.Libraries on a.LibraryId equals l.Id
            where a.ApiKey == apiKeyId
            select new { Artist = a, Library = l })
            .Include(x => x.Artist.UserArtists.Where(ua => ua.UserId == userId))
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (result == null)
        {
            return null;
        }

        var userArtist = result.Artist.UserArtists.FirstOrDefault();
        
        return new DatabaseDirectoryInfo(
            result.Artist.Id,
            result.Artist.ApiKey,
            result.Artist.SortName?.Substring(0, 1) ?? result.Artist.Name.Substring(0, 1),
            result.Artist.Name,
            $"artist_{result.Artist.ApiKey}",
            result.Artist.CalculatedRating,
            result.Artist.AlbumCount,
            result.Artist.PlayedCount,
            result.Artist.CreatedAt,
            result.Artist.LastUpdatedAt,
            result.Artist.LastPlayedAt,
            Path.Combine(result.Library.Path, result.Artist.Directory),
            userArtist?.StarredAt,
            userArtist?.Rating
        );
    }

    protected async Task<DatabaseDirectoryInfo?> DatabaseAlbumInfoForAlbumApiKey(Guid apiKeyId, int userId,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        
        var result = await (from a in scopedContext.Albums
            join aa in scopedContext.Artists on a.ArtistId equals aa.Id
            join l in scopedContext.Libraries on aa.LibraryId equals l.Id
            where a.ApiKey == apiKeyId
            select new { Album = a, Artist = aa, Library = l })
            .Include(x => x.Album.UserAlbums.Where(ua => ua.UserId == userId))
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (result == null)
        {
            return null;
        }

        var userAlbum = result.Album.UserAlbums.FirstOrDefault();
        
        return new DatabaseDirectoryInfo(
            result.Album.Id,
            result.Album.ApiKey,
            result.Album.SortName?.Substring(0, 1) ?? result.Album.Name.Substring(0, 1),
            result.Album.Name,
            $"album_{result.Album.ApiKey}",
            result.Album.CalculatedRating,
            0,
            result.Album.PlayedCount,
            result.Album.CreatedAt,
            result.Album.LastUpdatedAt,
            result.Album.LastPlayedAt,
            Path.Combine(result.Library.Path, result.Album.Directory),
            userAlbum?.StarredAt,
            userAlbum?.Rating
        );
    }

    protected async Task<DatabaseDirectoryInfo[]?> DatabaseSongInfosForAlbumApiKey(Guid apiKeyId, int userId,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        
        var results = await (from s in scopedContext.Songs
            join a in scopedContext.Albums on s.AlbumId equals a.Id
            join aa in scopedContext.Artists on a.ArtistId equals aa.Id
            join l in scopedContext.Libraries on aa.LibraryId equals l.Id
            where a.ApiKey == apiKeyId
            select new { Song = s, Album = a, Artist = aa, Library = l })
            .Include(x => x.Song.UserSongs.Where(us => us.UserId == userId))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var songInfos = new List<DatabaseDirectoryInfo>();
        
        foreach (var result in results)
        {
            var userSong = result.Song.UserSongs.FirstOrDefault();
            
            songInfos.Add(new DatabaseDirectoryInfo(
                result.Song.Id,
                result.Song.ApiKey,
                result.Song.TitleSort?.Substring(0, 1) ?? result.Song.Title.Substring(0, 1),
                result.Song.Title,
                $"song_{result.Song.ApiKey}",
                result.Song.CalculatedRating,
                0,
                result.Song.PlayedCount,
                result.Song.CreatedAt,
                result.Song.LastUpdatedAt,
                result.Song.LastPlayedAt,
                Path.Combine(result.Library.Path, result.Album.Directory),
                userSong?.StarredAt,
                userSong?.Rating
            ));
        }

        return songInfos.ToArray();
    }

    protected async Task<DatabaseSongIdsInfo?> DatabaseSongIdsInfoForSongApiKey(Guid apiKeyId,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        
        var result = await scopedContext.Songs
            .Include(s => s.Album)
            .ThenInclude(a => a.Artist)
            .Where(s => s.ApiKey == apiKeyId)
            .Select(s => new DatabaseSongIdsInfo(
                s.Id,
                s.ApiKey,
                s.Album.Id,
                s.Album.ApiKey,
                s.Album.Artist.Id,
                s.Album.Artist.ApiKey
            ))
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return result;
    }

    protected async Task<DatabaseSongScrobbleInfo?> DatabaseSongScrobbleInfoForSongApiKey(Guid apiKey,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        
        var result = await scopedContext.Songs
            .Include(s => s.Album)
            .ThenInclude(a => a.Artist)
            .Where(s => s.ApiKey == apiKey)
            .Select(s => new DatabaseSongScrobbleInfo(
                s.ApiKey,
                s.Album.Artist.Id,
                s.Album.Id,
                s.Id,
                s.Album.Artist.Name,
                s.Album.Name,
                Instant.FromDateTimeUtc(DateTime.UtcNow),
                s.Title,
                s.Duration,
                s.MusicBrainzId,
                s.SongNumber
            ))
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return result;
    }

    /// <summary>
    ///     Returns all albums created by reading songs and the total number of songs
    /// </summary>
    public async Task<OperationResult<(IEnumerable<Album>, int)>> AllAlbumsForDirectoryAsync(
        FileSystemDirectoryInfo fileSystemDirectoryInfo,
        IAlbumValidator albumValidator,
        ISongPlugin[] songPlugins,
        IMelodeeConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        var albums = new List<Album>();
        var messages = new List<string>();
        var viaPlugins = new List<string>();
        var songs = new List<Song>();

        var maxAlbumProcessingCount = configuration.GetValue<int>(SettingRegistry.ProcessingMaximumProcessingCount,
            value => value < 1 ? int.MaxValue : value);

        var dirInfo = new DirectoryInfo(fileSystemDirectoryInfo.Path);
        if (dirInfo.Exists)
        {
            using (Operation.At(LogEventLevel.Debug).Time("AllAlbumsForDirectoryAsync [{directoryInfo}]",
                       fileSystemDirectoryInfo.Name))
            {
                foreach (var fileSystemInfo in dirInfo.EnumerateFileSystemInfos("*.*", SearchOption.TopDirectoryOnly))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    var fsi = fileSystemInfo.ToFileSystemInfo();
                    foreach (var plugin in songPlugins.OrderBy(x => x.SortOrder))
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        if (plugin.DoesHandleFile(fileSystemDirectoryInfo, fsi))
                        {
                            var pluginResult =
                                await plugin.ProcessFileAsync(fileSystemDirectoryInfo, fsi, cancellationToken);
                            if (pluginResult.IsSuccess)
                            {
                                songs.Add(pluginResult.Data);
                                viaPlugins.Add(plugin.DisplayName);
                            }

                            messages.AddRange(pluginResult.Messages ?? []);
                        }

                        if (plugin.StopProcessing)
                        {
                            Logger.Warning("[{PluginName}] set stop processing flag.", plugin.DisplayName);
                            break;
                        }
                    }
                }

                foreach (var songsGroupedByAlbum in songs.GroupBy(x => x.AlbumId))
                {
                    foreach (var song in songsGroupedByAlbum)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        var foundAlbum = albums.FirstOrDefault(x => x.Id == songsGroupedByAlbum.Key);
                        if (foundAlbum != null)
                        {
                            albums.Remove(foundAlbum);
                            albums.Add(foundAlbum.MergeSongs([song]));
                        }
                        else
                        {
                            var songTotal = song.SongTotalNumber();
                            if (songTotal < 1)
                            {
                                songTotal = SafeParser.ToNumber<short>(songsGroupedByAlbum.Count());
                            }

                            var newAlbumTags = new List<MetaTag<object?>>
                            {
                                new()
                                {
                                    Identifier = MetaTagIdentifier.Album, Value = song.AlbumTitle(), SortOrder = 1
                                },
                                new()
                                {
                                    Identifier = MetaTagIdentifier.AlbumArtist, Value = song.AlbumArtist(),
                                    SortOrder = 2
                                },
                                new()
                                {
                                    Identifier = MetaTagIdentifier.DiscNumber, Value = 1, SortOrder = 4
                                },
                                new()
                                {
                                    Identifier = MetaTagIdentifier.DiscTotal, Value = 1, SortOrder = 4
                                },
                                new()
                                {
                                    Identifier = MetaTagIdentifier.OrigAlbumYear, Value = song.AlbumYear(),
                                    SortOrder = 100
                                },
                                new() { Identifier = MetaTagIdentifier.SongTotal, Value = songTotal, SortOrder = 101 }
                            };
                            var genres = songsGroupedByAlbum
                                .SelectMany(x => x.Tags ?? [])
                                .Where(x => x.Identifier == MetaTagIdentifier.Genre);
                            newAlbumTags.AddRange(genres
                                .GroupBy(x => x.Value)
                                .Select((genre, i) => new MetaTag<object?>
                                {
                                    Identifier = MetaTagIdentifier.Genre,
                                    Value = genre.Key,
                                    SortOrder = 5 + i
                                }));
                            var artistName = newAlbumTags.FirstOrDefault(x =>
                                    x.Identifier is MetaTagIdentifier.Artist or MetaTagIdentifier.AlbumArtist)?.Value
                                ?.ToString();
                            var newAlbum = new Album
                            {
                                Artist = Artist.NewArtistFromName(artistName ??
                                                                  throw new Exception("Invalid artist name")),
                                Images = songsGroupedByAlbum.Where(x => x.Images != null)
                                    .SelectMany(x => x.Images!)
                                    .DistinctBy(x => x.CrcHash).ToArray(),
                                Directory = fileSystemDirectoryInfo,
                                OriginalDirectory = fileSystemDirectoryInfo,
                                Tags = newAlbumTags,
                                Songs = songsGroupedByAlbum.OrderBy(x => x.SortOrder).ToArray(),
                                ViaPlugins = viaPlugins.Distinct().ToArray()
                            };
                            var validationResult = albumValidator.ValidateAlbum(newAlbum);
                            newAlbum.ValidationMessages = validationResult.Data.Messages ?? [];
                            newAlbum.Status = validationResult.Data.AlbumStatus;
                            newAlbum.StatusReasons = validationResult.Data.AlbumStatusReasons;
                            albums.Add(newAlbum);
                            if (albums.Count(x => x.IsValid) > maxAlbumProcessingCount)
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        return new OperationResult<(IEnumerable<Album>, int)>(messages)
        {
            Data = (albums, songs.Count)
        };
    }

    protected virtual OperationResult<(bool, IEnumerable<ValidationResult>?)> ValidateModel<T>(T? dataToValidate)
    {
        if (dataToValidate != null)
        {
            var ctx = new ValidationContext(dataToValidate);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(dataToValidate, ctx, validationResults, true))
            {
                return new OperationResult<(bool, IEnumerable<ValidationResult>?)>
                {
                    Data = (false, validationResults),
                    Type = OperationResponseType.ValidationFailure
                };
            }
        }

        return new OperationResult<(bool, IEnumerable<ValidationResult>?)>
        {
            Data = (dataToValidate != null, null)
        };
    }
}
