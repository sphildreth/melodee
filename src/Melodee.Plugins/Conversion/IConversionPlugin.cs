using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;

namespace Melodee.Plugins.Conversion;

public interface IConversionPlugin
{
    bool DoesHandleFile(FileSystemInfo fileSystemInfo);
    
    bool StopProcessing { get; }
    
    Task<OperationResult<FileSystemInfo>> ProcessFileAsync(FileSystemInfo fileSystemInfo, CancellationToken cancellationToken = default);
}