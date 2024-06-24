namespace Melodee.Plugins.MetaData.Release;

public abstract class ReleaseMetaDataBase : MetaDataBase
{
    public override bool DoesHandleFile(FileSystemInfo fileSystemInfo) => false;
}