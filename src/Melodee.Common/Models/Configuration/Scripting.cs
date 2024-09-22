namespace Melodee.Common.Models.Configuration;

[Serializable]
public sealed record Scripting
{
    public string? PreDiscoveryScript { get; set; }

    public string? PostDiscoveryScript { get; set; }
}
