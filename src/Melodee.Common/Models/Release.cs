using System.Text.Json.Serialization;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Utility;

namespace Melodee.Common.Models;

/// <summary>
///     This is a representation of a Release (a published collection of Tracks) including all known MetaData.
/// </summary>
[Serializable]
public sealed record Release
{
    public const string JsonFileName = "melodee.json";

    public long UniqueId => SafeParser.Hash(this.Artist(), this.ReleaseYear().ToString(), this.ReleaseTitle());

    public DateTimeOffset Created { get; set; } = DateTimeOffset.Now;
    
    public DateTimeOffset? Modified { get; set; }

    /// <summary>
    ///     What plugins were utilized in discovering this release.
    /// </summary>
    public required IEnumerable<string> ViaPlugins { get; set; }

    /// <summary>
    ///     This is the directory where the Release was created, it will not be the "Staging" or "Library" directory where
    ///     there Release is moved to once processed.
    /// </summary>
    public required FileSystemDirectoryInfo OriginalDirectory { get; init; }

    public FileSystemDirectoryInfo? Directory { get; set; }

    public IEnumerable<ImageInfo>? Images { get; set; }

    public IEnumerable<MetaTag<object?>>? Tags { get; set; }

    public IEnumerable<Track>? Tracks { get; set; }

    public IEnumerable<string> Messages { get; set; } = [];

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ReleaseStatus Status { get; set; } = ReleaseStatus.Invalid;

    public IEnumerable<ReleaseFile> Files { get; set; } = [];

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
    
    public string UniqueIdSummary => $"{this.Artist()} : {this.ReleaseYear()} : {this.ReleaseTitle()}";
    
    public string DisplaySummary => $"{this.MediaCountValue().ToStringPadLeft(2)} : {this.TrackTotalValue().ToStringPadLeft(3)} : {this.ReleaseTitle()}";

    public Release MergeTracks(IEnumerable<Track> pluginResultData)
    {
        var tracks = new List<Track>(Tracks ?? []);
        foreach (var track in pluginResultData)
        {
            if (!tracks.Contains(track))
            {
                tracks.Add(track);
            }
        }

        var releaseTags = Tags?.ToList();
        var trackTotalTag = releaseTags?.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.TrackTotal);
        if (trackTotalTag != null)
        {
            releaseTags!.Remove(trackTotalTag);
            releaseTags.Add(new MetaTag<object?>
            {
                Identifier = MetaTagIdentifier.TrackTotal,
                Value = tracks.Count,
                SortOrder = trackTotalTag.SortOrder,
                StyleClass = trackTotalTag.StyleClass
            });
        }

        return this with { Tracks = tracks.ToArray(), Tags = releaseTags!.ToArray() };
    }

    public override string ToString()
    {
        return $"UniqueId [{UniqueId}] Status [{Status}] TrackCount [{Tracks?.Count() ?? 0}] ImageCount [{Images?.Count() ?? 0}] Directory [{Directory}]";
    }

    public Release Merge(Release otherRelease)
    {
        var files = new List<ReleaseFile>(Files);
        var images = new List<ImageInfo>(Images ?? []);
        var messages = new List<string>(Messages);
        var tags = new List<MetaTag<object?>>(Tags ?? []);
        var tracks = new List<Track>(Tracks ?? []);
        var viaPlugins = new List<string>(ViaPlugins);

        if (otherRelease.Images != null)
        {
            foreach (var image in otherRelease.Images)
            {
                if (!images.Any(x => x.IsCrcHashMatch(image.CrcHash)))
                {
                    images.Add(image);
                }
            }
        }

        if (otherRelease.Tags != null)
        {
            foreach (var tag in otherRelease.Tags)
            {
                if (tags.FirstOrDefault(x => x.Identifier == tag.Identifier)?.Value?.ToString() != tag.Value?.ToString())
                {
                    tags.Add(tag);
                }
            }
        }

        if (otherRelease.Tracks != null)
        {
            foreach (var track in otherRelease.Tracks)
            {
                if (!tracks.Select(x => x.UniqueId).Contains(track.UniqueId))
                {
                    tracks.Add(track);
                }
            }
        }

        if (otherRelease.ViaPlugins.Any())
        {
            foreach (var plugin in otherRelease.ViaPlugins)
            {
                if (!viaPlugins.Contains(plugin))
                {
                    viaPlugins.Add(plugin);
                }
            }
        }

        if (otherRelease.Files.Any())
        {
            foreach (var file in otherRelease.Files)
            {
                if (!files.Contains(file))
                {
                    files.Add(file);
                }
            }
        }

        messages.AddRange(otherRelease.Messages);

        return new Release
        {
            OriginalDirectory = OriginalDirectory,
            Tags = tags,
            ViaPlugins = viaPlugins,
            Files = files.ToArray(),
            Images = images.ToArray(),
            Messages = messages.Distinct().ToArray(),
            SortOrder = SortOrder,
            Status = ReleaseStatus.NotSet,
            Tracks = tracks.ToArray()
        };
    }

    public void SetTagValue(MetaTagIdentifier identifier, object? value, bool? doSetTrackValue = true)
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
        if (doSetTrackValue ?? true)
        {
            foreach (var track in Tracks ?? [])
            {
                SetTrackTagValue(track.TrackId, identifier, value);
            }
        }
    }

    public void RemoveTrackTagValue(long trackId, MetaTagIdentifier identifier)
    {
        SetTrackTagValue(trackId, identifier, null);
    }

    public void SetTrackTagValue(long trackId, MetaTagIdentifier identifier, object? value)
    {
        var tracks = (Tracks ?? []).ToList();
        var track = tracks.FirstOrDefault(x => x.TrackId == trackId);
        if (track != null)
        {
            var tags = (track.Tags ?? []).ToList();
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

            tracks.Remove(track);
            tracks.Add(track with { Tags = tags.ToArray() });
            Tracks = tracks.ToArray();
        }
    }
}
