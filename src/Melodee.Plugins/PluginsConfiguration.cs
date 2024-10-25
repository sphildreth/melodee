namespace Melodee.Plugins;

public record PluginsConfiguration(Dictionary<string, object?> Configuration) : IPluginsConfiguration
{
}
