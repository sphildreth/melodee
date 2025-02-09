using Melodee.Common.Enums;
using Melodee.Common.Models;

namespace Melodee.Common.Plugins.MetaData.Song;

public interface ISongFileUpdatePlugin
{
    OperationResult<bool> UpdateFile(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo file, MetaTagIdentifier identifier, object? value);
}
