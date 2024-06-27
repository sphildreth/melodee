using Melodee.Common.Models;

namespace Melodee.Plugins.MetaData.Track;

public interface ITrackPlugin : IPlugin
{
    bool DoesHandleFile(FileSystemFileInfo fileSystemInfo);
    
    bool StopProcessing { get; }
    
    Task<OperationResult<Common.Models.Track>> ProcessFileAsync(FileSystemFileInfo fileSystemInfo, CancellationToken cancellationToken = default);
}