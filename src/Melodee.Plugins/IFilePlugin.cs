using Melodee.Common.Models;

namespace Melodee.Plugins;

public interface IFilePlugin : IPlugin
{
    bool DoesHandleFile(FileSystemInfo fileSystemInfo);
    
    bool StopProcessing { get; }
    
    Task<OperationResult<Release>> ProcessFileAsync(FileSystemInfo fileSystemInfo, CancellationToken cancellationToken = default);
}