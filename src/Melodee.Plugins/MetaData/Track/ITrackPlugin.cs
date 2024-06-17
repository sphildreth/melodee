using Melodee.Common.Models;

namespace Melodee.Plugins.MetaData.Track;

public interface ITrackPlugin : IPlugin
{
    bool DoesHandleFile(FileSystemInfo fileSystemInfo);
    
    bool StopProcessing { get; }
    
    Task<OperationResult<Common.Models.Track>> ProcessFileAsync(FileSystemInfo fileSystemInfo, CancellationToken cancellationToken = default);
}