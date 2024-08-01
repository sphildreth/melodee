using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;

namespace Melodee.Plugins.MetaData.Directory.Models;

[Serializable]
public sealed record CueSheet
{
    public bool IsValid => !string.IsNullOrWhiteSpace(FileSystemFileInfo.Name) &&  Tracks.Any() && Tags.Any() && TrackIndexes.Any() && FileSystemFileInfo.Exists(FileSystemDirectoryInfo);
    
    public required FileSystemFileInfo FileSystemFileInfo { get; init; }
    
    public required FileSystemDirectoryInfo FileSystemDirectoryInfo { get; init; }
    
    public required IEnumerable<Common.Models.Track> Tracks { get; init; }
    
    public required IEnumerable<CueIndex> TrackIndexes { get; init; }
    
    public required IEnumerable<MetaTag<object?>> Tags { get; init; }

}