using Melodee.Common.Models;

namespace Melodee.Plugins.MetaData.Directory;

public interface IDirectoryPlugin : IPlugin
{
    bool StopProcessing { get; }
    
    Task<OperationResult<bool>> ProcessDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default);
}