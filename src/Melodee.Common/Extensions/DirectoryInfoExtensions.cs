using Melodee.Common.Models;

namespace Melodee.Common.Extensions;

public static class DirectoryInfoExtensions
{
    public static bool IsSameDirectory(this DirectoryInfo directoryInfo, string path)
    {
        var di = directoryInfo.Name.TrimEnd('\\').TrimEnd('/');
        var p = path.TrimEnd('\\').TrimEnd('/');
        return string.Equals(di, p, StringComparison.OrdinalIgnoreCase);
    }
    
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
