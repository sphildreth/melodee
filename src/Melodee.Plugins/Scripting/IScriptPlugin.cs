using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using DirectoryInfo = System.IO.DirectoryInfo;

namespace Melodee.Plugins.Scripting;

public interface IScriptPlugin : IPlugin
{
    bool StopProcessing { get; }
    
    Task<OperationResult<bool>> ProcessAsync(DirectoryInfo directoryInfo, CancellationToken cancellationToken = default);
}