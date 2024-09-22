using Melodee.Common.Models;

namespace Melodee.Plugins.Conversion;

public interface IConversionPlugin : IPlugin
{
    bool StopProcessing { get; }
    bool DoesHandleFile(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo);

    Task<OperationResult<FileSystemFileInfo>> ProcessFileAsync(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo, CancellationToken cancellationToken = default);
}
