namespace Melodee.Common.Models;

/// <summary>
/// This is a representation of a Release (a published collection of Tracks) including all known MetaData.
/// </summary>
public sealed record Release
{
    public required DirectoryInfo DirectoryInfo { get; init; }
    
    public required IEnumerable<FileInfo> FileInfos { get; init; }
    
    public IEnumerable<ImageInfo>? Images { get; init; }
    
    public IEnumerable<MetaTag<object>>? Tags { get; init; }
}