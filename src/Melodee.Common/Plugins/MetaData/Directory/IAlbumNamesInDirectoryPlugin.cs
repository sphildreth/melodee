using Melodee.Common.Models;

namespace Melodee.Common.Plugins.MetaData.Directory;

/// <summary>
///     Returns the unique names of albums in a given directory.
/// </summary>
public interface IAlbumNamesInDirectoryPlugin
{
    OperationResult<string[]> AlbumNamesInDirectory(FileSystemDirectoryInfo directoryInfo);
}
