using Melodee.Common.Utility;

namespace Melodee.Common.Models;

[Serializable]
public sealed record FileSystemFileInfo
{
    public long UniqueId => SafeParser.Hash(Path);
    
    /// <summary>
    /// Full path to File
    /// </summary>
    public required string Path { get; init; }
    
    public required string Name { get; init; }
    
    public required long Size { get; init; }

}