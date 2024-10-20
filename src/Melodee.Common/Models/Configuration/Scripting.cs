namespace Melodee.Common.Models.Configuration;

[Serializable]
public sealed record Scripting
{
    public bool Disabled { get; init; }
    
    public string? PreDiscoveryScript { get; set; }

    public string? PostDiscoveryScript { get; set; }
}
