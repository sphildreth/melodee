namespace Melodee.Plugins;

public interface IPluginsConfiguration
{
    Dictionary<string, object?> Configuration { get; }
}
