using Melodee.Common.Models;

namespace Melodee.Plugins.MetaData.Song;

public class NullSongPlugin : ISongPlugin
{
    public string Id => "DF5B8F59-D5AF-478E-A5AE-4A275841B3AA";
    
    public string DisplayName => nameof(NullSongPlugin);
    
    public bool IsEnabled { get; set; } = false;
    
    public int SortOrder => 0;
    public bool StopProcessing { get; }
    public bool DoesHandleFile(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo)
    {
        throw new NotImplementedException();
    }

    public Task<OperationResult<Common.Models.Song>> ProcessFileAsync(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<OperationResult<bool>> UpdateSongAsync(FileSystemDirectoryInfo directoryInfo, Common.Models.Song song, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
