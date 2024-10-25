using Melodee.Common.Models;


namespace Melodee.Plugins.MetaData;

public abstract class MetaDataBase(IPluginsConfiguration configuration)
{
    protected Dictionary<string, object?> Configuration { get; } = configuration.Configuration;

    public abstract string Id { get; }

    public abstract string DisplayName { get; }

    public abstract bool IsEnabled { get; set; }

    public abstract int SortOrder { get; }

    public bool StopProcessing { get; internal set; }

    public abstract bool DoesHandleFile(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo);
}
