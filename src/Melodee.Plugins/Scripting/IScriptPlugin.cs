using Melodee.Common.Models;
using Melodee.Plugins.Conversion.Models;
using DirectoryInfo = System.IO.DirectoryInfo;

namespace Melodee.Plugins.Scripting;

public interface IScriptPlugin : IPlugin
{
    bool StopProcessing { get; }
    
    Task<OperationResult<bool>> ProcessAsync(DirectoryInfo directoryInfo, ProcessFileOptions processFileOptions, CancellationToken cancellationToken = default);
}