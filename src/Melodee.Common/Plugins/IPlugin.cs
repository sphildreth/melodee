namespace Melodee.Common.Plugins;

public interface IPlugin
{
    string Id { get; }

    string DisplayName { get; }

    bool IsEnabled { get; set; }

    /// <summary>
    ///     Ranking of Plugin, plugins is sorted in ascending order with the lowest number executed first.
    /// </summary>
    int SortOrder { get; }
}
