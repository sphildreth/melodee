using System.Text.Json.Serialization;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Utility;

namespace Melodee.Common.Models;

/// <summary>
///     This is a representation of a Album (a published collection of Songs) including all known MetaData.
/// </summary>
[Serializable]
public sealed record Album
{
    public const string JsonFileName = "melodee.json";

    public long UniqueId => SafeParser.Hash(this.Artist(), this.AlbumYear().ToString(), this.AlbumTitle());

    public DateTimeOffset Created { get; set; } = DateTimeOffset.Now;
    
    public DateTimeOffset? Modified { get; set; }

    /// <summary>
    ///     What plugins were utilized in discovering this Album.
    /// </summary>
    public required IEnumerable<string> ViaPlugins { get; set; }

    /// <summary>
    ///     This is the directory where the Album was created, it will not be the "Staging" or "Library" directory where the Album is moved to once processed.
    /// </summary>
    public required FileSystemDirectoryInfo OriginalDirectory { get; init; }

    public FileSystemDirectoryInfo? Directory { get; set; }
    
    /// <summary>
    /// The full path to the melodee.json file.
    /// </summary>
    public string? MelodeeDataFileName { get; set; }

    public IEnumerable<ImageInfo>? Images { get; set; }

    public IEnumerable<MetaTag<object?>>? Tags { get; set; }

    public IEnumerable<Song>? Songs { get; set; }

    public IEnumerable<string> Messages { get; set; } = [];

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AlbumStatus Status { get; set; } = AlbumStatus.Invalid;

    public IEnumerable<AlbumFile> Files { get; set; } = [];

    public int SortOrder { get; set; }

    public string SortValue
    {
        get
        {
            if (SortOrder > 0)
            {
                return SortOrder.ToString();
            }

            return this.ToDirectoryName();
        }
    }
    
    public string UniqueIdSummary => $"{this.Artist()} : {this.AlbumYear()} : {this.AlbumTitle()}";
    
    public string DisplaySummary => $"{this.MediaCountValue().ToStringPadLeft(2)} : {this.SongTotalValue().ToStringPadLeft(3)} : {this.AlbumTitle()}";

    public async Task<string?> CoverImageBase64Async(CancellationToken cancellationToken = default)
    {
        if (Images == null || !Images.Any())
        {
            return null;
        }
        var cover = Images.FirstOrDefault(x => x.PictureIdentifier == PictureIdentifier.Front);
        if (cover != null)
        {
            var imageBytes = await File.ReadAllBytesAsync(cover.FileInfo?.FullName(Directory) ?? string.Empty, cancellationToken);
            return $"data:image/jpeg;base64,{ Convert.ToBase64String(imageBytes)}";   
        }
        return null;
    }    
    
    public Album MergeSongs(IEnumerable<Song> pluginResultData)
    {
        var songs = new List<Song>(Songs ?? []);
        foreach (var song in pluginResultData)
        {
            if (!songs.Contains(song))
            {
                songs.Add(song);
            }
        }

        var albumTags = Tags?.ToList();
        var songTotalTag = albumTags?.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.SongTotal);
        if (songTotalTag != null)
        {
            albumTags!.Remove(songTotalTag);
            albumTags.Add(new MetaTag<object?>
            {
                Identifier = MetaTagIdentifier.SongTotal,
                Value = songs.Count,
                SortOrder = songTotalTag.SortOrder,
                StyleClass = songTotalTag.StyleClass
            });
        }

        return this with { Songs = songs.ToArray(), Tags = albumTags!.ToArray() };
    }

    public override string ToString()
    {
        return $"UniqueId [{UniqueId}] Status [{Status}] SongCount [{Songs?.Count() ?? 0}] ImageCount [{Images?.Count() ?? 0}] Directory [{Directory}]";
    }

    public Album Merge(Album otherAlbum)
    {
        var files = new List<AlbumFile>(Files);
        var images = new List<ImageInfo>(Images ?? []);
        var messages = new List<string>(Messages);
        var tags = new List<MetaTag<object?>>(Tags ?? []);
        var songs = new List<Song>(Songs ?? []);
        var viaPlugins = new List<string>(ViaPlugins);

        if (otherAlbum.Images != null)
        {
            foreach (var image in otherAlbum.Images)
            {
                if (!images.Any(x => x.IsCrcHashMatch(image.CrcHash)))
                {
                    images.Add(image);
                }
            }
        }

        if (otherAlbum.Tags != null)
        {
            foreach (var tag in otherAlbum.Tags)
            {
                if (tags.FirstOrDefault(x => x.Identifier == tag.Identifier)?.Value?.ToString() != tag.Value?.ToString())
                {
                    tags.Add(tag);
                }
            }
        }

        if (otherAlbum.Songs != null)
        {
            foreach (var song in otherAlbum.Songs)
            {
                if (Songs != null && !Songs.Select(x => x.UniqueId).Contains(song.UniqueId))
                {
                    songs.Add(song);
                }
            }
        }

        if (otherAlbum.ViaPlugins.Any())
        {
            foreach (var plugin in otherAlbum.ViaPlugins)
            {
                if (!viaPlugins.Contains(plugin))
                {
                    viaPlugins.Add(plugin);
                }
            }
        }

        if (otherAlbum.Files.Any())
        {
            foreach (var file in otherAlbum.Files)
            {
                if (!files.Contains(file))
                {
                    files.Add(file);
                }
            }
        }

        messages.AddRange(otherAlbum.Messages);

        return new Album
        {
            Files = files.ToArray(),
            Images = images.ToArray(),
            Messages = messages.Distinct().ToArray(),
            MelodeeDataFileName = MelodeeDataFileName,
            OriginalDirectory = OriginalDirectory,
            Songs = (Songs ?? Array.Empty<Song>()).ToArray(),
            SortOrder = SortOrder,
            Status = AlbumStatus.NotSet,
            Tags = tags,
            ViaPlugins = viaPlugins
        };
    }

    public void SetTagValue(MetaTagIdentifier identifier, object? value, bool? doSetSongValue = true)
    {
        var tags = (Tags ?? []).ToList();
        var existingTag = tags.FirstOrDefault(x => x.Identifier == identifier);
        if (existingTag != null)
        {
            tags.Remove(existingTag);
        }

        if (value != null)
        {
            tags.Add(new MetaTag<object?>
            {
                Identifier = identifier,
                OriginalValue = existingTag?.OriginalValue,
                SortOrder = existingTag?.SortOrder ?? 0,
                StyleClass = existingTag?.StyleClass ?? StyleClass.Normal,
                Value = value
            });
        }

        Tags = tags.ToArray();
        if (doSetSongValue ?? true)
        {
            foreach (var song in Songs ?? [])
            {
                SetSongTagValue(song.SongId, identifier, value);
            }
        }
    }

    public void RemoveSongTagValue(long songId, MetaTagIdentifier identifier)
    {
        SetSongTagValue(songId, identifier, null);
    }

    public void SetSongTagValue(long songId, MetaTagIdentifier identifier, object? value)
    {
        var songs = (Songs ?? []).ToList();
        var song = songs.FirstOrDefault(x => x.SongId == songId);
        if (song != null)
        {
            var tags = (song.Tags ?? []).ToList();
            var existingTag = tags.FirstOrDefault(x => x.Identifier == identifier);
            if (existingTag != null)
            {
                tags.Remove(existingTag);
            }

            if (value != null)
            {
                tags.Add(new MetaTag<object?>
                {
                    Identifier = identifier,
                    OriginalValue = existingTag?.OriginalValue,
                    SortOrder = existingTag?.SortOrder ?? 0,
                    StyleClass = existingTag?.StyleClass ?? StyleClass.Normal,
                    Value = value
                });
            }

            songs.Remove(song);
            songs.Add(song with { Tags = tags.ToArray() });
            Songs = songs.ToArray();
        }
    }
}
