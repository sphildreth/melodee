using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;

namespace Melodee.Plugins.MetaData;

public abstract class MetaDataBase(Configuration configuration)
{
    protected Configuration Configuration { get; } = configuration;

    public abstract string Id { get; }

    public abstract string DisplayName { get; }

    public abstract bool IsEnabled { get; set; }

    public abstract int SortOrder { get; }

    public bool StopProcessing { get; internal set; }

    public abstract bool DoesHandleFile(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo);
}
