namespace Melodee.Common.Models.Extensions;

public static class FileSystemInfoExtensions
{
    public static FileSystemFileInfo ToFileSystemInfo(this System.IO.FileSystemInfo fileInfo)
    {
        var fi = new System.IO.FileInfo(fileInfo.FullName);
        return fi.ToFileSystemInfo();
    }
}