using Melodee.Common.Models;

namespace Melodee.Common.Plugins.Scripting;

public interface IScriptPlugin : IPlugin
{
    bool StopProcessing { get; }

    Task<OperationResult<bool>> ProcessAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo,
        CancellationToken cancellationToken = default);
}
