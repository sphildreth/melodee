namespace Melodee.Common.Models;

public sealed record Release
{
    public required DirectoryInfo DirectoryInfo { get; init; }
    
    public required IEnumerable<FileInfo> FileInfos { get; init; }
    
    public IEnumerable<ImageInfo>? Images { get; init; }
    
    public IEnumerable<MetaTag<object>>? Tags { get; init; }
}