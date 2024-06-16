using System.Text.Json.Serialization;
using Melodee.Common.Enums;

namespace Melodee.Common.Models;

[Serializable]
public sealed record FileInfo
{
    [JsonIgnore]
    public required DirectoryInfo DirectoryInfo { get; init; }
    
    public required string Path { get; init; }
    
    public required string Name { get; init; }
    
    public required string Extension { get; init; }
    
    public IEnumerable<MetaTag<object>>? Tags { get; init; }
    
    public IEnumerable<ImageInfo>? Images { get; init; }
    public required ReleaseStatus Status { get; init; } = ReleaseStatus.NeedsAttention;
}