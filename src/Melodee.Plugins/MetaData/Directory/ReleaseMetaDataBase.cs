using Melodee.Common.Models.Configuration;

namespace Melodee.Plugins.MetaData.Directory;

public abstract class AlbumMetaDataBase(Configuration configuration) : MetaDataBase(configuration)
{
    //public override bool DoesHandleFile(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo) => false;
}
