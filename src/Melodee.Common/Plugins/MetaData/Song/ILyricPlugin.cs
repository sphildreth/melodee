using Melodee.Common.Models;
using Melodee.Common.Models.OpenSubsonic;

namespace Melodee.Common.Plugins.MetaData.Song;

public interface ILyricPlugin : IPlugin
{
    Task<OperationResult<Lyrics?>> GetLyricsAsync(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo, CancellationToken token = default);
    
    Task<OperationResult<LyricsList?>> GetLyricListAsync(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo, CancellationToken token = default);
}
