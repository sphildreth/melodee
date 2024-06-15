namespace Melodee.Common.Models;

[Serializable]
public sealed record Configuration
{
    public required string InboundDirectory { get; init; }
    
    public required string LibraryDirectory { get; init; }
}