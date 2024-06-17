namespace Melodee.Common.Models;

[Serializable]
public sealed record Track
{
    public required FileInfo FileInfo { get; init; }
    
    public IEnumerable<ImageInfo>? Images { get; init; }
    
    public IEnumerable<MetaTag<object>>? Tags { get; init; }
}