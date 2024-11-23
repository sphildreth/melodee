using System.ComponentModel.DataAnnotations;
using Dapper;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Data.Models.DTOs;
using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Plugins.MetaData.Song;
using Melodee.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using SerilogTimings;

namespace Melodee.Services;

public abstract class ServiceBase(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory)
{
    public const string CacheName = "melodee";
    protected static TimeSpan DefaultCacheDuration = TimeSpan.FromDays(1);

    protected ILogger Logger { get; } = logger;
    protected ICacheManager CacheManager { get; } = cacheManager;
    protected IDbContextFactory<MelodeeDbContext> ContextFactory { get; } = contextFactory;

    protected async Task<DatabaseSongIdsInfo?> DatabaseSongIdsInfoForSongApiKey(Guid apiKeyId, CancellationToken cancellationToken = default)
    {
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbConn = scopedContext.Database.GetDbConnection();
            var sql = """
                      select s."Id" as SongId, s."ApiKey" as SongApiKey, ad."Id" as AlbumDiscId, 
                             a."Id" as "AlbumId", a."ApiKey" as AlbumApiKey, aa."Id" as AlbumArtistId, aa."ApiKey" as AlbumArtistApiKey
                      from "Songs" s 
                      left join "AlbumDiscs" ad on (s."AlbumDiscId" = ad."Id")
                      left join "Albums" a on (ad."AlbumId" = a."Id")
                      left join "Artists" aa on (a."ArtistId" = aa."Id")
                      where s."ApiKey" = @apiKeyId;
                      """;
            return await dbConn.QuerySingleOrDefaultAsync<DatabaseSongIdsInfo>(sql, new { apiKeyId }).ConfigureAwait(false);
        }
    }

    protected async Task<DatabaseSongScrobbleInfo?> DatabaseSongScrobbleInfoForSongApiKey(Guid apiKeyId, CancellationToken cancellationToken = default)
    {
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbConn = scopedContext.Database.GetDbConnection();
            var sql = """
                      select s."ApiKey" as SongApiKey, aa."Name" as ArtistName, a."Name" as AlbumTitle, now() as TimePlayed, 
                             s."Title" as "SongTitle", s."Duration" as SongDuration, s."MusicBrainzId" as SongMusicBrainzId, s."SongNumber" as SongNumber
                      from "Songs" s 
                      left join "AlbumDiscs" ad on (s."AlbumDiscId" = ad."Id")
                      left join "Albums" a on (ad."AlbumId" = a."Id")
                      left join "Artists" aa on (a."ArtistId" = aa."Id")
                      where s."ApiKey" = @apiKeyId;
                      """;
            return await dbConn.QuerySingleOrDefaultAsync<DatabaseSongScrobbleInfo>(sql, new { apiKeyId }).ConfigureAwait(false);
        }
    }

    public async Task<OperationResult<(IEnumerable<Album>, int)>> AllAlbumsForDirectoryAsync(
        FileSystemDirectoryInfo fileSystemDirectoryInfo,
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
                            using (Operation.At(LogEventLevel.Debug).Time("File [{File}] Plugin [{Plugin}]",
                                       fileSystemInfo.Name, plugin.DisplayName))
                            {
                                var pluginResult = await plugin.ProcessFileAsync(fileSystemDirectoryInfo, fsi, cancellationToken);
                                if (pluginResult.IsSuccess)
                                {
                                    songs.Add(pluginResult.Data);
                                    viaPlugins.Add(plugin.DisplayName);
                                }

                                messages.AddRange(pluginResult.Messages);
                            }
                        }

                        if (plugin.StopProcessing)
                        {
                            break;
                        }
                    }
                }

                foreach (var songsGroupedByAlbum in songs.GroupBy(x => x.AlbumUniqueId))
                {
                    foreach (var song in songsGroupedByAlbum)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        var foundAlbum = albums.FirstOrDefault(x => x.UniqueId == songsGroupedByAlbum.Key);
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
                                songTotal = songsGroupedByAlbum.Count();
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
                                    Identifier = MetaTagIdentifier.DiscNumber, Value = song.MediaNumber(), SortOrder = 4
                                },
                                new()
                                {
                                    Identifier = MetaTagIdentifier.DiscTotal, Value = song.MediaTotalNumber(), SortOrder = 4
                                },
                                new()
                                {
                                    Identifier = MetaTagIdentifier.OrigAlbumYear, Value = song.AlbumYear(),
                                    SortOrder = 100
                                },
                                new() { Identifier = MetaTagIdentifier.SongTotal, Value = songTotal, SortOrder = 101 }
                            };
                            var genres = songsGroupedByAlbum
                                .SelectMany(x => x.Tags ?? Array.Empty<MetaTag<object?>>())
                                .Where(x => x.Identifier == MetaTagIdentifier.Genre);
                            newAlbumTags.AddRange(genres
                                .GroupBy(x => x.Value)
                                .Select((genre, i) => new MetaTag<object?>
                                {
                                    Identifier = MetaTagIdentifier.Genre,
                                    Value = genre.Key,
                                    SortOrder = 5 + i
                                }));
                            var newAlbum = new Album
                            {
                                Images = songsGroupedByAlbum.Where(x => x.Images != null)
                                    .SelectMany(x => x.Images!)
                                    .DistinctBy(x => x.CrcHash).ToArray(),
                                Directory = fileSystemDirectoryInfo,
                                OriginalDirectory = fileSystemDirectoryInfo,
                                Tags = newAlbumTags,
                                Songs = songsGroupedByAlbum.OrderBy(x => x.SortOrder).ToArray(),
                                ViaPlugins = viaPlugins.Distinct().ToArray()
                            };
                            newAlbum.Status = newAlbum.IsValid(configuration.Configuration).Item1 ? AlbumStatus.Ok : AlbumStatus.Invalid;
                            albums.Add(newAlbum);
                            if (albums.Count(x => x.Status == AlbumStatus.Ok) > maxAlbumProcessingCount)
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
