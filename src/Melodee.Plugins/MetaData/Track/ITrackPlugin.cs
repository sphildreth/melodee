using Melodee.Common.Models;

namespace Melodee.Plugins.MetaData.Track;

public interface ITrackPlugin : IPlugin
{
    bool DoesHandleFile(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo);
    
    bool StopProcessing { get; }
    
    Task<OperationResult<Common.Models.Track>> ProcessFileAsync(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo, CancellationToken cancellationToken = default);
}