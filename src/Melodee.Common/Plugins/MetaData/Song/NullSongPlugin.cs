using Melodee.Common.Enums;
using Melodee.Common.Models;

namespace Melodee.Common.Plugins.MetaData.Song;

public class NullSongPlugin : ISongPlugin
{
    public string Id => "DF5B8F59-D5AF-478E-A5AE-4A275841B3AA";

    public string DisplayName => nameof(NullSongPlugin);

    public bool IsEnabled { get; set; } = false;

    public int SortOrder => 0;

    public bool StopProcessing { get; } = false;

    public bool DoesHandleFile(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo)
    {
        return false;
    }

    public Task<OperationResult<Models.Song>> ProcessFileAsync(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new OperationResult<Models.Song>
        {
            Data = new Models.Song
            {
                ArtistId = Guid.Empty,
                AlbumId = Guid.Empty,
                CrcHash = string.Empty,
                File = new FileSystemFileInfo
                {
                    Name = string.Empty,
                    Size = 0
                }
            }
        });
    }

    public Task<OperationResult<bool>> UpdateSongAsync(FileSystemDirectoryInfo directoryInfo, Models.Song song, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new OperationResult<bool> { Data = true });
    }
}
