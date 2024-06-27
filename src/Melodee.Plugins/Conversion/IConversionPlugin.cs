using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;

namespace Melodee.Plugins.Conversion;

public interface IConversionPlugin
{
    string DisplayName { get; }
    
    bool DoesHandleFile(FileSystemFileInfo fileSystemInfo);
    
    bool StopProcessing { get; }
    
    Task<OperationResult<FileSystemFileInfo>> ProcessFileAsync(FileSystemFileInfo fileSystemInfo, CancellationToken cancellationToken = default);
}