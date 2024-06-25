using Melodee.Common.Models;
using DirectoryInfo = Melodee.Common.Models.DirectoryInfo;

namespace Melodee.Plugins.Scripting;

public interface IScriptPlugin : IPlugin
{
    bool StopProcessing { get; }
    
    Task<OperationResult<bool>> ProcessAsync(DirectoryInfo directoryInfo, CancellationToken cancellationToken = default);
}