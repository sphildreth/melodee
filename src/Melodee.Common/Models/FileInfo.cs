using Melodee.Common.Enums;

namespace Melodee.Common.Models;

[Serializable]
public sealed record FileInfo
{
    public required DirectoryInfo DirectoryInfo { get; init; }
    
    public required string Path { get; init; }
    
    public required string Name { get; init; }
    
    public required FileInfoStatus Status { get; init; }
}