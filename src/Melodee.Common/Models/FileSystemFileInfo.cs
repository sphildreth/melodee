using Melodee.Common.Utility;

namespace Melodee.Common.Models;

/// <summary>
///     Contains only file information, does not contain path information.
/// </summary>
public sealed record FileSystemFileInfo
{
    public long UniqueId => SafeParser.Hash(Name, Size.ToString());

    public required string Name { get; set; }

    public required long Size { get; init; }

    public string? OriginalName { get; init; }

    public override string ToString()
    {
        return $"UniqueId [{UniqueId}] Size [{Size}] Name [{Name}]";
    }
}
