using Melodee.Common.Models;

namespace Melodee.Plugins.MetaData.Directory.Models;

[Serializable]
public sealed record CueSheet
{
    public bool IsValid => !string.IsNullOrWhiteSpace(FileSystemFileInfo.Name) &&  Tracks.Any() && Tags.Any() && TrackIndexes.Any();
    
    public required FileSystemFileInfo FileSystemFileInfo { get; init; }
    
    public required IEnumerable<Common.Models.Track> Tracks { get; init; }
    
    public required IEnumerable<CueIndex> TrackIndexes { get; init; }
    
    public required IEnumerable<MetaTag<object?>> Tags { get; init; }

}