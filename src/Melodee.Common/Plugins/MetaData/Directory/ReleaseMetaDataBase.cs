using Melodee.Common.Configuration;

namespace Melodee.Common.Plugins.MetaData.Directory;

public abstract class AlbumMetaDataBase(IMelodeeConfiguration configuration) : MetaDataBase(configuration)
{
    //public override bool DoesHandleFile(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo) => false;
}
