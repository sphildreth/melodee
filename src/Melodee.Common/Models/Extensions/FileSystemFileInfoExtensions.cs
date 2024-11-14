using Serilog;

namespace Melodee.Common.Models.Extensions;

public static class FileSystemFileInfoExtensions
{
    public static FileInfo ToFileInfo(this FileSystemFileInfo fileSystemFileInfo) => new(fileSystemFileInfo.FullPath);

    public static bool Exists(this FileSystemFileInfo fileSystemFileInfo)
    {
        return File.Exists(fileSystemFileInfo.FullPath);
    }

    public static string Extension(this FileSystemFileInfo fileSystemFileInfo)
    {
        return Path.GetExtension(fileSystemFileInfo.FullPath);
    }
}
