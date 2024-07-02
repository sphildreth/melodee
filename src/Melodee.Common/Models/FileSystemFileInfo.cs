using Melodee.Common.Utility;

namespace Melodee.Common.Models;

[Serializable]
public sealed record FileSystemFileInfo
{
    public long UniqueId => SafeParser.Hash(Name, Size.ToString());
    
    public required string Name { get; init; }
    
    public required long Size { get; init; }

}