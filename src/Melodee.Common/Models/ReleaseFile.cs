using Melodee.Common.Enums;

namespace Melodee.Common.Models;

[Serializable]
public sealed record ReleaseFile
{
    public long ReleaseUniqueId { get; set; }
    
    public required ReleaseFileType ReleaseFileType { get; init; }
    
    public required string ProcessedByPlugin { get; init; }
    
    public required FileSystemInfo FileSystemInfo { get; init; }
}