namespace Melodee.Common.Models.Configuration;

[Serializable]
public sealed record Scripting
{
    public string? PreDiscoveryScript { get; init; }
    
    public string? PostDiscoveryScript { get; init; }
}