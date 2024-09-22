using Melodee.Common.Extensions;

namespace Melodee.Common.Models.Extensions;

public static class FileSystemInfoExtensions
{
    public static FileSystemDirectoryInfo ToDirectorySystemInfo(this FileSystemInfo fileInfo)
    {
        var dir = new DirectoryInfo(fileInfo.FullName);
        return new FileSystemDirectoryInfo
        {
            Path = dir.FullName,
            Name = dir.Name
        };
    }

    public static FileSystemFileInfo ToFileSystemInfo(this FileSystemInfo fileInfo)
    {
        var fi = new FileInfo(fileInfo.FullName);
        return FileInfoExtensions.ToFileSystemInfo(fi);
    }
}
