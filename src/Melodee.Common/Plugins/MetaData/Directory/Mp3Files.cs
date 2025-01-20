using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Plugins.MetaData.Song;
using Melodee.Common.Plugins.Validation;
using Melodee.Common.Serialization;
using Melodee.Common.Utility;
using Serilog;
using Serilog.Events;
using SerilogTimings;

namespace Melodee.Common.Plugins.MetaData.Directory;

/// <summary>
///     Creates an album in a directory for MP3 files grouped by Album Title
/// </summary>
public class Mp3Files(
    IEnumerable<ISongPlugin> songPlugins,
    IAlbumValidator albumValidator,
    ISerializer serializer,
    ILogger logger,
    IMelodeeConfiguration configuration) : AlbumMetaDataBase(configuration), IDirectoryPlugin
{
    private const string HandlesExtension = "MP3";

    public override string Id => "4015E7C8-240F-4FC2-A40D-372168C78C98";

    public override string DisplayName => nameof(Mp3Files);

    public override bool IsEnabled { get; set; } = true;

    public override int SortOrder { get; } = 0;

    public async Task<OperationResult<int>> ProcessDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default)
    {
        var processedFileCount = 0;

        var albums = new List<Album>();
        var messages = new List<string>();
        var viaPlugins = new List<string>
        {
            DisplayName
        };
        var songs = new List<Common.Models.Song>();

        var maxAlbumProcessingCount = MelodeeConfiguration.GetValue<int>(SettingRegistry.ProcessingMaximumProcessingCount, value => value < 1 ? int.MaxValue : value);

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
                    }
                }

                var songMaxMediaNumber = 1;
                if (songs.Count > 0)
                {
                    var songMediaNumbers = songs.Select(x => x.MediaNumber()).Distinct().ToArray();
                    songMaxMediaNumber = songMediaNumbers.Length != 0 ? songMediaNumbers.Max() : 1;
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
                                    Identifier = MetaTagIdentifier.DiscTotal, Value = songMaxMediaNumber, SortOrder = 4
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
                            var artistName = newAlbumTags.FirstOrDefault(x => x.Identifier is MetaTagIdentifier.Artist or MetaTagIdentifier.AlbumArtist)?.Value?.ToString();
                            var newAlbum = new Album
                            {
                                Artist = Artist.NewArtistFromName(artistName ?? throw new Exception("Invalid artist name")),
                                AlbumType  = song.AlbumTitle().TryToDetectAlbumType(), 
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

        // Save all album files to given directory
        var serialized = string.Empty;
        foreach (var album in albums)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                serialized = serializer.Serialize(album);
            }
            catch (Exception e)
            {
                logger.Error(e, "Error serializing album [{Album}]", album.ToString());
            }

            await File.WriteAllTextAsync(Path.Combine(fileSystemDirectoryInfo.FullName(), album.ToMelodeeJsonName(MelodeeConfiguration, true)), serialized, cancellationToken);
        }

        return new OperationResult<int>(messages)
        {
            Data = processedFileCount
        };
    }
    
    public override bool DoesHandleFile(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo)
    {
        return fileSystemInfo.Extension(directoryInfo).DoStringsMatch(HandlesExtension);
    }
}
