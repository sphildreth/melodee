using Melodee.Common.Models;
using Melodee.Plugins.Conversion.Models;

namespace Melodee.Plugins.Conversion;

public interface IConversionPlugin
{
    bool DoesHandleFile(FileSystemInfo fileSystemInfo);
    
    bool StopProcessing { get; }
    
    Task<OperationResult<FileSystemInfo>> ProcessFileAsync(FileSystemInfo fileSystemInfo, ProcessFileOptions processFileOptions, CancellationToken cancellationToken = default);
}