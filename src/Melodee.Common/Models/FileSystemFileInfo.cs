using Melodee.Common.Utility;

namespace Melodee.Common.Models;

public sealed record FileSystemFileInfo
{
    public long UniqueId => SafeParser.Hash(Name, Size.ToString());

    public required string Name { get; set; }
    
    public required string FullPath { get; set; }

    public required long Size { get; init; }

    public string? FullPathOriginalName { get; init; }

    public override string ToString()
    {
        return $"UniqueId [{UniqueId}] Size [{Size}] Name [{Name}]";
    }
}
