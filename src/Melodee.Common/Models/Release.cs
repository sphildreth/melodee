using Melodee.Common.Enums;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Utility;

namespace Melodee.Common.Models;

/// <summary>
/// This is a representation of a Release (a published collection of Tracks) including all known MetaData.
/// </summary>
[Serializable]
public sealed record Release
{
    private long? _uniqueId;

    public long UniqueId => (_uniqueId ??= SafeParser.Hash($"{this.Artist()}{this.ReleaseYear()}{this.ReleaseTitle}"));

    /// <summary>
    /// What plugins were utilized in discovering this release.
    /// </summary>
    public required IEnumerable<string> ViaPlugins { get; set; }    
    
    public required FileSystemDirectoryInfo Directory { get; init; }

    public IEnumerable<ImageInfo>? Images { get; set; }

    public IEnumerable<MetaTag<object?>>? Tags { get; init; }

    public IEnumerable<Track>? Tracks { get; set; }

    public IEnumerable<string> Messages { get; set; } = [];

    public ReleaseStatus Status { get; set; } = ReleaseStatus.New;

    public IEnumerable<ReleaseFile> Files { get; set; } = [];

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
            releaseTags.Add(new MetaTag<object?>()
            {
                Identifier = MetaTagIdentifier.TrackTotal,
                Value = tracks.Count,
                SortOrder = trackTotalTag.SortOrder,
                StyleClass = trackTotalTag.StyleClass
            });
        }
        return this with { Tracks = tracks.ToArray(), Tags = releaseTags!.ToArray() };
    }
    
    public int SortOrder { get; set; }

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
                if (!images.Contains(image))
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
            Directory = Directory,
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

}