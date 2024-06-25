namespace Melodee.Common.Models.Configuration;

[Serializable]
public sealed record PluginProcessOptions
{
    public bool DoDeleteOriginal { get; set; } = true;
}