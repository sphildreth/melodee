using Melodee.Common.Models;

namespace Melodee.Common.Extensions;

public static class DirectoryInfoExtensions
{
    public static FileSystemDirectoryInfo? ToFileSystemDirectoryInfo(this DirectoryInfo? directoryInfo)
    {
        if (directoryInfo == null)
        {
            return null;
        }

        return new FileSystemDirectoryInfo
        {
            Path = directoryInfo.FullName,
            Name = directoryInfo.Name
        };
    }
}
