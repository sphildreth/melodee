using Melodee.Common.Enums;
using Melodee.Common.Models;

namespace Melodee.Common.Plugins.MetaData.Song;

public interface ISongPlugin : IPlugin
{
    bool StopProcessing { get; }
    
    bool DoesHandleFile(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo);

    Task<OperationResult<Models.Song>> ProcessFileAsync(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo, CancellationToken cancellationToken = default);

    Task<OperationResult<bool>> UpdateSongAsync(FileSystemDirectoryInfo directoryInfo, Models.Song song, CancellationToken cancellationToken = default);
}
