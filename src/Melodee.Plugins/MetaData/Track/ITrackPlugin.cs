using Melodee.Common.Models;

namespace Melodee.Plugins.MetaData.Track;

public interface ITrackPlugin : IPlugin
{
    bool StopProcessing { get; }
    bool DoesHandleFile(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo);

    Task<OperationResult<Common.Models.Track>> ProcessFileAsync(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo, CancellationToken cancellationToken = default);

    Task<OperationResult<bool>> UpdateTrackAsync(FileSystemDirectoryInfo directoryInfo, Common.Models.Track track, CancellationToken cancellationToken = default);
}
