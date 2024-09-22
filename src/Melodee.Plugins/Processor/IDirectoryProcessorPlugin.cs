using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;

namespace Melodee.Plugins.Processor;

public interface IDirectoryProcessorPlugin : IPlugin
{
    event EventHandler<Release>? OnReleaseDiscovered;
    
    Task<OperationResult<int>> ProcessDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default);
}
