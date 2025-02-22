using System.Diagnostics;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Plugins.MetaData.Song;
using Melodee.Common.Plugins.Validation;
using Melodee.Common.Serialization;
using Melodee.Common.Services.Scanning;
using Melodee.Common.Utility;
using Serilog;
using Serilog.Events;
using SerilogTimings;
using Artist = Melodee.Common.Models.Artist;

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
        var errors = new List<Exception>();
        var viaPlugins = new List<string>
        {
            DisplayName
        };
        var songs = new List<Common.Models.Song>();

        var maxAlbumProcessingCount = MelodeeConfiguration.GetValue<int>(SettingRegistry.ProcessingMaximumProcessingCount, value => value < 1 ? int.MaxValue : value);

        if (fileSystemDirectoryInfo.Exists())
        {
            using (Operation.At(LogEventLevel.Debug).Time("[{PluginName}] ProcessDirectoryAsync [{directoryInfo}]", DisplayName, fileSystemDirectoryInfo.Name))
            {
                foreach(var fileSystemInfo in fileSystemDirectoryInfo.AllMediaTypeFileInfos(SearchOption.TopDirectoryOnly))
                {
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
                                viaPlugins.Add($"{nameof(Mp3Files)}:{plugin.DisplayName}");
                            }
                            else
                            {
                                Trace.WriteLine($"Unable to process file: [{fsi}]");
                            }
                            errors.AddRange(pluginResult.Errors ?? []);
                            messages.AddRange(pluginResult.Messages ?? []);
                            processedFileCount++;
                        }
                    }
                }
                
                await HandleDuplicates(fileSystemDirectoryInfo, songs.ToArray(), cancellationToken);   
                EnsureSortOrderSet(songs.ToArray());

                foreach (var songsGroupedByAlbum in songs.GroupBy(x => x.SongArtistAlbumUniqueId()))
                {
                    foreach (var song in songsGroupedByAlbum)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }
                        var foundAlbum = albums.FirstOrDefault(x => x.Artist.NameNormalized == (song.AlbumArtist().ToNormalizedString() ?? song.AlbumArtist()) && x.AlbumTitle() == song.AlbumTitle());
                        if (foundAlbum != null)
                        {
                            albums.Remove(foundAlbum);
                            albums.Add(foundAlbum.MergeSongs([song]));
                        }
                        else
                        {
                            var songTotal =  SafeParser.ToNumber<short>(songsGroupedByAlbum.Count());
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
                                    Identifier = MetaTagIdentifier.DiscTotal, Value = 1, SortOrder = 4
                                },
                                new()
                                {
                                    Identifier = MetaTagIdentifier.RecordingYear, Value = song.AlbumYear(),
                                    SortOrder = 100
                                },
                                new() { Identifier = MetaTagIdentifier.SongTotal, Value = songTotal, SortOrder = 101 }
                            };
                            var albumDate = song.AlbumDate();
                            if (albumDate != null)
                            {
                                newAlbumTags.Add(new()
                                {
                                    Identifier = MetaTagIdentifier.AlbumDate, Value = albumDate,
                                    SortOrder = 100
                                }); 
                            }
                            var genres = songsGroupedByAlbum
                                .SelectMany(x => x.Tags ?? Array.Empty<MetaTag<object?>>())
                                .Where(x => x.Identifier == MetaTagIdentifier.Genre);
                            newAlbumTags.AddRange(genres
                                .GroupBy(x => x.Value)
                                .Select((genre, i) => new MetaTag<object?>
                                {
                                    Identifier = MetaTagIdentifier.Genre,
                                    Value = genre.Key?.ToString()?.CleanStringAsIs(),
                                    SortOrder = 5 + i
                                }));
                            var artistName = newAlbumTags.FirstOrDefault(x => x.Identifier is MetaTagIdentifier.Artist or MetaTagIdentifier.AlbumArtist)?.Value?.ToString();
                            var newAlbum = new Album
                            {
                                Artist = Artist.NewArtistFromName(artistName ?? throw new Exception("Invalid artist name")),
                                AlbumType = song.AlbumTitle().TryToDetectAlbumType(),
                                Images = songsGroupedByAlbum.Where(x => x.Images != null)
                                    .SelectMany(x => x.Images!)
                                    .DistinctBy(x => x.CrcHash).ToArray(),
                                Directory = fileSystemDirectoryInfo,
                                OriginalDirectory = fileSystemDirectoryInfo,
                                Tags = newAlbumTags,
                                Songs = songsGroupedByAlbum.OrderBy(x => x.SortOrder).ToArray(),
                                ViaPlugins = viaPlugins.Distinct().ToArray()
                            };
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
            var validationResult = albumValidator.ValidateAlbum(album);
            album.ValidationMessages = validationResult.Data.Messages ?? [];
            album.Status = validationResult.Data.AlbumStatus;
            album.StatusReasons = validationResult.Data.AlbumStatusReasons;
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
            Errors = errors.ToArray(),
            Data = processedFileCount
        };
    }

    private void EnsureSortOrderSet(Common.Models.Song[] songs)
    {
        Trace.WriteLine($"Ensuring sort order is set on songs...");
        foreach (var song in songs)
        {
            song.SortOrder = (song.SongNumber() + (song.MediaNumber() * MediaEditService.SortOrderMediaMultiplier)) - MediaEditService.SortOrderMediaMultiplier;
        }
    }

    private async Task HandleDuplicates(FileSystemDirectoryInfo fileSystemDirectoryInfo, Common.Models.Song[] seenSongs, CancellationToken cancellationToken = default)
    {
        Trace.WriteLine($"Checking for duplicate files in [{fileSystemDirectoryInfo.FullName()}]...");

        if (seenSongs.Length == 0)
        {
            return;
        }

        var ss = seenSongs.ToList();
        var foundDuplicates = await fileSystemDirectoryInfo.FindDuplicatesAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        foreach (var dup in foundDuplicates.SelectMany(x => x.Value))
        {
            dup.Delete();
            Trace.WriteLine($"Deleted duplicate: {dup.FullName}");
            ss.RemoveAll(x => x.File.FullName(fileSystemDirectoryInfo) == dup.FullName);
        }
        
        var duplicateSongs = ss.GroupBy(x => x.DuplicateHashCheck).Where(x => x.Count() > 1).ToArray();
        if (duplicateSongs.Any())
        {
            foreach (var duplicateGroup in duplicateSongs)
            {
                var bestSong = Common.Models.Song.IdentityBestAndMergeOthers(duplicateGroup.ToArray());
                var duplicateSong = duplicateGroup.Where(x => x.Id != bestSong.Id).ToArray();
                foreach (var ds in duplicateSong)
                {
                    var duplicateSongFile = ds.File.FullName(fileSystemDirectoryInfo);
                    File.Delete(duplicateSongFile);
                    Trace.WriteLine($"Deleted duplicate song: {duplicateSongFile}");
                }
            }
        }
        
    }

    public override bool DoesHandleFile(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo)
    {
        return fileSystemInfo.Extension(directoryInfo).DoStringsMatch(HandlesExtension);
    }
}
