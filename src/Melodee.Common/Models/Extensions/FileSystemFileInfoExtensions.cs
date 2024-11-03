using Serilog;

namespace Melodee.Common.Models.Extensions;

public static class FileSystemFileInfoExtensions
{
    public static string FullName(this FileSystemFileInfo fileSystemFileInfo, FileSystemDirectoryInfo directoryInfo)
    {
        return Path.Combine(directoryInfo.Path, fileSystemFileInfo.Name);
    }

    public static string FullOriginalName(this FileSystemFileInfo fileSystemFileInfo, FileSystemDirectoryInfo directoryInfo)
    {
        return Path.Combine(directoryInfo.Path, fileSystemFileInfo.OriginalName ?? fileSystemFileInfo.Name);
    }

    public static bool Exists(this FileSystemFileInfo fileSystemFileInfo, FileSystemDirectoryInfo directoryInfo)
    {
        return File.Exists(fileSystemFileInfo.FullName(directoryInfo));
    }

    public static string Extension(this FileSystemFileInfo fileSystemFileInfo, FileSystemDirectoryInfo directoryInfo)
    {
        return Path.GetExtension(fileSystemFileInfo.FullName(directoryInfo));
    }
}
