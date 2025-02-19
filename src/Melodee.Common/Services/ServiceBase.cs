using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Dapper;
using IdSharp.Common.Utils;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Data.Models.DTOs;
using Melodee.Common.Enums;
using Melodee.Common.MessageBus.Events;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Models.OpenSubsonic;
using Melodee.Common.Plugins.Conversion.Image;
using Melodee.Common.Plugins.MetaData.Song;
using Melodee.Common.Plugins.Processor;
using Melodee.Common.Plugins.Validation;
using Melodee.Common.Serialization;
using Melodee.Common.Services.Interfaces;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;
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
    /// This is required for Mocking in unit tests.
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

    protected async Task<AlbumUpdatedEvent?> ProcessExistingDirectoryMoveMergeAsync(IMelodeeConfiguration configuration, ISerializer serializer, Album albumToMove, string existingAlbumPath, CancellationToken cancellationToken = default)
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
                Trace.WriteLine("No image found in existing directory. Copying all images from Album...");
                foreach (var image in albumToMove.Images)
                {
                    if (image.FileInfo != null)
                    {
                        var imagePath = image.FileInfo.FullName(albumToMoveDir);
                        if(File.Exists(imagePath))
                        {
                            File.Move(imagePath,Path.Combine(existingDir.FullName, image.FileInfo.Name));
                            Logger.Debug("[{ServiceName}] :\u2502: moving new image [{FileName}]", nameof(LibraryService), image.FileInfo.Name);
                        }
                    }

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
                    foreach (var existingWithSameFileName in existingWithSameFileNames)
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
                        Trace.WriteLine($"Existing image [{existingWithSameFileName.ImageFileName}] to be overwritten by image [{toFile}]...");
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
                        Trace.WriteLine($"Moving image [{imageToMove.ImageFileName}] to [{toFile}]...");
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
            var imageValidator = new ImageValidator(configuration);
            var imageConvertor = new ImageConvertor(configuration);
            var atlMetTag = new AtlMetaTag(new MetaTagsProcessor(configuration, serializer), imageConvertor, imageValidator, configuration);
            var existingSongsFileInfos = albumToMoveDir.AllMediaTypeFileInfos().ToArray();
            var existingSongs = new List<Song>();
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
                var existingForSongToMove = existingSongs.FirstOrDefault(x => x.SongNumber() == songToMove.SongNumber());
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
                if (File.Exists(songToMove.File.FullName(albumToMoveDir)) && !File.Exists(toFile))
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
        if (modifiedExistingDirectory)
        {
            return new AlbumUpdatedEvent(null, existingAlbumPath);
        }

        return null;
    }


    protected async Task<OperationResult<bool>> UpdateArtistAggregateValuesByIdAsync(int artistId, CancellationToken cancellationToken = default)
    {
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbConn = scopedContext.Database.GetDbConnection();
            var sql = """
                      UPDATE "Artists" a
                      SET "AlbumCount" = (select COUNT(*) from "Albums" where "ArtistId" = a."Id"), "LastUpdatedAt" = NOW()
                      WHERE a."Id" = @artistId
                      AND "AlbumCount" <> (select COUNT(*) from "Albums" where "ArtistId" = a."Id");
                                                     
                      UPDATE "Artists" a
                      SET "SongCount" = (
                      	select COUNT(s.*)
                      	from "Songs" s 
                        join "Albums" aa on (s."AlbumId" = aa."Id")	
                      	where aa."ArtistId" = a."Id"
                      ), "LastUpdatedAt" = NOW()
                      where "SongCount" <> (
                      	select COUNT(s.*)
                      	from "Songs" s 
                        join "Albums" aa on (s."AlbumId" = aa."Id")	
                      	where aa."ArtistId" = a."Id"
                      )
                      AND a."Id" = @artistId;
                      """;
            var result = await dbConn
                .ExecuteAsync(sql, new { artistId })
                .ConfigureAwait(false);

            return new OperationResult<bool>
            {
                Data = result > 0
            };
        }
    }


    protected async Task<OperationResult<bool>> UpdateLibraryAggregateStatsByIdAsync(int libraryId, CancellationToken cancellationToken = default)
    {
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbConn = scopedContext.Database.GetDbConnection();
            var sql = """
                      UPDATE "Libraries" l 
                      set "ArtistCount" = (select count(*) from "Artists" where "LibraryId" = l."Id"),
                          "AlbumCount" = (select count(aa.*) 
                          	from "Albums" aa 
                          	join "Artists" a on (a."Id" = aa."ArtistId") 
                          	where a."LibraryId" = l."Id"),
                          "SongCount" = (select count(s.*) 
                          	from "Songs" s
                          	join "Albums" aa on (s."AlbumId" = aa."Id") 
                          	join "Artists" a on (a."Id" = aa."ArtistId") 
                          	where a."LibraryId" = l."Id"),
                      	"LastUpdatedAt" = now()
                      where l."Id" = @libraryId;
                      """;

            var result = await dbConn
                .ExecuteAsync(sql, new { libraryId })
                .ConfigureAwait(false);

            return new OperationResult<bool>
            {
                Data = result > 0
            };
        }
    }


    protected async Task<AlbumList2[]> AlbumListForArtistApiKey(Guid artistApiKey, int userId, CancellationToken cancellationToken)
    {
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbConn = scopedContext.Database.GetDbConnection();
            var sql = """
                      SELECT 
                          'album_' || cast(a."ApiKey" as varchar(50)) as "Id",
                          a."Name" as "Album",
                          a."Name" as "Title",
                          a."Name" as "Name",
                          'album_' || cast(a."ApiKey" as varchar(50)) as "CoverArt",
                          a."SongCount",
                          a."CreatedAt" as "CreatedRaw",
                          a."Duration"/1000 as "Duration",
                          a."PlayedCount",
                          'artist_' || cast(aa."ApiKey" as varchar(50)) as "ArtistId",
                          aa."Name" as "Artist",
                          DATE_PART('year', a."ReleaseDate"::date) as "Year",
                          a."Genres",
                          (SELECT COUNT(*) FROM "UserAlbums" WHERE "IsStarred" AND "AlbumId" = a."Id") as "UserStarredCount",
                          ua."IsStarred" as "Starred",
                          ua."Rating" as "UserRating"
                      FROM "Albums" a 
                      JOIN "Artists" aa on (a."ArtistId" = aa."Id")
                      LEFT JOIN "UserAlbums" ua on (a."Id" = ua."AlbumId" and ua."UserId" = @userId)
                      WHERE aa."ApiKey" = @artistApiKey;
                      """;
            return (await dbConn.QueryAsync<AlbumList2>(sql, new { userId, artistApiKey }).ConfigureAwait(false)).ToArray();
        }
    }

    protected async Task<DatabaseDirectoryInfo?> DatabaseArtistInfoForArtistApiKey(Guid apiKeyId, int userId, CancellationToken cancellationToken = default)
    {
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbConn = scopedContext.Database.GetDbConnection();
            var sql = """
                      select a."Id", a."ApiKey", LEFT(a."SortName", 1) as "Index", a."Name", 'artist_' || a."ApiKey" as "CoverArt", a."CalculatedRating", a."AlbumCount", a."PlayedCount" as "PlayCount", a."CreatedAt" as "CreatedAt", a."LastUpdatedAt" as "LastUpdatedAt",
                             a."LastPlayedAt" as "Played", l."Path" || a."Directory" as "Directory", ua."StarredAt" as "UserStarred", ua."Rating" as "UserRating"
                      from "Artists" a
                      join "Libraries" l on (a."LibraryId" = l."Id")
                      left join "UserArtists" ua on (a."Id" = ua."ArtistId" and ua."UserId" = @userId)
                      where a."ApiKey" = @apiKeyId;
                      """;
            return await dbConn.QuerySingleOrDefaultAsync<DatabaseDirectoryInfo>(sql, new { userId, apiKeyId }).ConfigureAwait(false);
        }
    }

    protected async Task<DatabaseDirectoryInfo?> DatabaseAlbumInfoForAlbumApiKey(Guid apiKeyId, int userId, CancellationToken cancellationToken = default)
    {
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbConn = scopedContext.Database.GetDbConnection();
            var sql = """
                      select a."Id", a."ApiKey", LEFT(a."SortName", 1) as "Index", a."Name", 'album_' || a."ApiKey" as "CoverArt", a."CalculatedRating", 0 as "AlbumCount", a."PlayedCount" as "PlayCount",a."CreatedAt" as "CreatedAt", a."LastUpdatedAt" as "LastUpdatedAt", 
                             a."LastPlayedAt" as "Played", l."Path" || a."Directory" as "Directory", ua."StarredAt" as "UserStarred", ua."Rating" as "UserRating"
                      from "Albums" a
                      join "Artists" aa on (a."ArtistId" = aa."Id")
                      join "Libraries" l on (aa."LibraryId" = l."Id")
                      left join "UserAlbums" ua on (a."Id" = ua."AlbumId" and ua."UserId" = @userId)
                      where a."ApiKey" = @apiKeyId;
                      """;
            return await dbConn.QuerySingleOrDefaultAsync<DatabaseDirectoryInfo>(sql, new { userId, apiKeyId }).ConfigureAwait(false);
        }
    }

    protected async Task<DatabaseDirectoryInfo[]?> DatabaseSongInfosForAlbumApiKey(Guid apiKeyId, int userId, CancellationToken cancellationToken = default)
    {
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbConn = scopedContext.Database.GetDbConnection();
            var sql = """
                      select s."Id", s."ApiKey", LEFT(s."TitleSort", 1) as "Index", s."Title" as "Name", 'song_' || s."ApiKey" as "CoverArt", s."CalculatedRating", 0 as "AlbumCount", s."PlayedCount" as "PlayCount", s."CreatedAt" as "CreatedAt", s."LastUpdatedAt" as "LastUpdatedAt", 
                             s."LastPlayedAt" as "Played", l."Path" || a."Directory" as "Directory", us."StarredAt" as "UserStarred", us."Rating" as "UserRating"
                      from "Songs" s
                      join "Albums" a on (s."AlbumId" = a."Id") 
                      join "Artists" aa on (a."ArtistId" = aa."Id")
                      join "Libraries" l on (aa."LibraryId" = l."Id")    
                      left join "UserSongs" us on (s."Id" = us."SongId" and us."UserId" = @userId)
                      where a."ApiKey" = @apiKeyId;
                      """;
            return (await dbConn.QueryAsync<DatabaseDirectoryInfo>(sql, new { userId, apiKeyId }).ConfigureAwait(false)).ToArray();
        }
    }

    protected async Task<DatabaseSongIdsInfo?> DatabaseSongIdsInfoForSongApiKey(Guid apiKeyId, CancellationToken cancellationToken = default)
    {
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbConn = scopedContext.Database.GetDbConnection();
            var sql = """
                      select s."Id" as SongId, s."ApiKey" as SongApiKey,  
                             a."Id" as "AlbumId", a."ApiKey" as AlbumApiKey, aa."Id" as AlbumArtistId, aa."ApiKey" as AlbumArtistApiKey
                      from "Songs" s 
                      left join "Albums" a on (s."AlbumId" = a."Id")
                      left join "Artists" aa on (a."ArtistId" = aa."Id")
                      where s."ApiKey" = @apiKeyId;
                      """;
            return await dbConn.QuerySingleOrDefaultAsync<DatabaseSongIdsInfo>(sql, new { apiKeyId }).ConfigureAwait(false);
        }
    }

    protected async Task<DatabaseSongScrobbleInfo?> DatabaseSongScrobbleInfoForSongApiKey(Guid apiKey, CancellationToken cancellationToken = default)
    {
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbConn = scopedContext.Database.GetDbConnection();
            var sql = """
                      select s."ApiKey" as SongApiKey, aa."Id" as ArtistId, a."Id" as AlbumId, s."Id" as SongId, 
                             aa."Name" as ArtistName, a."Name" as AlbumTitle, now() as TimePlayed, 
                             s."Title" as "SongTitle", s."Duration" as SongDuration, s."MusicBrainzId" as SongMusicBrainzId, s."SongNumber" as SongNumber
                      from "Songs" s 
                      left join "Albums" a on (s."AlbumId" = a."Id")
                      left join "Artists" aa on (a."ArtistId" = aa."Id")
                      where s."ApiKey" = @apiKeyId;
                      """;
            return await dbConn.QuerySingleOrDefaultAsync<DatabaseSongScrobbleInfo>(sql, new { apiKeyId = apiKey }).ConfigureAwait(false);
        }
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

        var maxAlbumProcessingCount = configuration.GetValue<int>(SettingRegistry.ProcessingMaximumProcessingCount, value => value < 1 ? int.MaxValue : value);

        var dirInfo = new DirectoryInfo(fileSystemDirectoryInfo.Path);
        if (dirInfo.Exists)
        {
            using (Operation.At(LogEventLevel.Debug).Time("AllAlbumsForDirectoryAsync [{directoryInfo}]", fileSystemDirectoryInfo.Name))
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
                            var pluginResult = await plugin.ProcessFileAsync(fileSystemDirectoryInfo, fsi, cancellationToken);
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
                                    Identifier = MetaTagIdentifier.AlbumArtist, Value = song.AlbumArtist(), SortOrder = 2
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
                            var artistName = newAlbumTags.FirstOrDefault(x => x.Identifier is MetaTagIdentifier.Artist or MetaTagIdentifier.AlbumArtist)?.Value?.ToString();
                            var newAlbum = new Album
                            {
                                Artist = Artist.NewArtistFromName(artistName ?? throw new Exception("Invalid artist name")),
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
