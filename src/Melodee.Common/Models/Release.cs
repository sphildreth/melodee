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
    public long UniqueId => SafeParser.Hash($"{this.Artist()}{this.ReleaseYear()}{this.ReleaseTitle}");

    /// <summary>
    /// What plugins were utilized in discovering this release.
    /// </summary>
    public required IEnumerable<string> ViaPlugins { get; set; }    
    
    public required DirectoryInfo DirectoryInfo { get; init; }

    public IEnumerable<ImageInfo>? Images { get; set; }

    public IEnumerable<MetaTag<object?>>? Tags { get; init; }

    public IEnumerable<Track>? Tracks { get; init; }

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

        return this with { Tracks = tracks };
    }
    
    public int SortOrder { get; set; }

    public Release Merge(Release pluginResultData)
    {
        var images = new List<ImageInfo>(Images ?? []);
        var tags = new List<MetaTag<object>>(Tags ?? []);
        var tracks = new List<Track>(Tracks ?? []);
        var viaPlugins = new List<string>(ViaPlugins ?? []);

        if (UniqueId != pluginResultData.UniqueId)
        {
            if (pluginResultData.Images != null)
            {
                foreach (var image in pluginResultData.Images)
                {
                    if (!images.Contains(image))
                    {
                        images.Add(image);
                    }
                }
            }

            if (pluginResultData.Tags != null)
            {
                foreach (var tag in pluginResultData.Tags)
                {
                    if (!tags.Contains(tag))
                    {
                        tags.Add(tag);
                    }
                }
            }

            if (pluginResultData.Tracks != null)
            {
                foreach (var track in pluginResultData.Tracks)
                {
                    if (!tracks.Contains(track))
                    {
                        tracks.Add(track);
                    }
                }
            }
            
            if (pluginResultData.ViaPlugins.Any())
            {
                foreach (var plugin in pluginResultData.ViaPlugins)
                {
                    if (!viaPlugins.Contains(plugin))
                    {
                        viaPlugins.Add(plugin);
                    }
                }
            }            
        }

        return new Release
        {
            DirectoryInfo = DirectoryInfo,
            Images = images,
            Tags = tags,
            Tracks = tracks,
            ViaPlugins = viaPlugins
        };
    }
}