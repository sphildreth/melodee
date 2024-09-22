namespace Melodee.Plugins;

public interface IPlugin
{
    string Id { get; }

    string DisplayName { get; }

    bool IsEnabled { get; set; }

    int SortOrder { get; }
}
