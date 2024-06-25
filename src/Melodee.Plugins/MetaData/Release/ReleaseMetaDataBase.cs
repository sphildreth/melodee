using Melodee.Common.Models.Configuration;

namespace Melodee.Plugins.MetaData.Release;

public abstract class ReleaseMetaDataBase(Configuration configuration) : MetaDataBase(configuration)
{
    public override bool DoesHandleFile(FileSystemInfo fileSystemInfo) => false;
}