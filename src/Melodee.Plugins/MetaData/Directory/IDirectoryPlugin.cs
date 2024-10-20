using Melodee.Common.Models;

namespace Melodee.Plugins.MetaData.Directory;

public interface IDirectoryPlugin : IPlugin
{
    bool StopProcessing { get; }

    /// <summary>
    ///     Process the given directory and return the number of processed files by the plugin.
    /// </summary>
    Task<OperationResult<int>> ProcessDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default);
}
