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
    
    public required DirectoryInfo DirectoryInfo { get; init; }
    
    public required IEnumerable<FileInfo> FileInfos { get; init; }
    
    public IEnumerable<ImageInfo>? Images { get; init; }
    
    public IEnumerable<MetaTag<object>>? Tags { get; init; }
    
    public IEnumerable<Track>? Tracks { get; init; }

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
    
    public Release Merge(Release pluginResultData)
    {
        var fileInfos = new List<FileInfo>(FileInfos);
        var images = new List<ImageInfo>(Images ?? []);
        var tags = new List<MetaTag<object>>(Tags ?? []);
        var tracks = new List<Track>(Tracks ?? []);
        
        if (UniqueId != pluginResultData.UniqueId)
        {
            foreach (var fileInfo in pluginResultData.FileInfos)
            {
                if (!fileInfos.Contains(fileInfo))
                {
                    fileInfos.Add(fileInfo);
                }
            }
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
        }
        return new Release
        {
            DirectoryInfo = DirectoryInfo,
            FileInfos = fileInfos,
            Images = images,
            Tags = tags,
            Tracks = tracks
        };

    }
}