using Melodee.Common.Extensions;

namespace Melodee.Common.Models.Extensions;

public static class FileSystemInfoExtensions
{
    /// <summary>
    /// Returns a Directory System Info for the given FileInfo
    /// </summary>
    public static FileSystemDirectoryInfo ToDirectorySystemInfo(this FileSystemInfo fileInfo)
    {
        var fi = fileInfo as FileInfo;
        return new FileSystemDirectoryInfo
        {
            Path = fi?.Directory?.FullName ?? fileInfo.FullName,
            Name = fi?.Directory?.Name ?? fileInfo.Name,
        };
    }

    /// <summary>
    /// Returns a File System Info for the given FileInfo
    /// </summary>
    public static FileSystemFileInfo ToFileSystemInfo(this FileSystemInfo fileInfo)
    {
        var fi = new FileInfo(fileInfo.FullName);
        return FileInfoExtensions.ToFileSystemInfo(fi);
    }
    
   
}
