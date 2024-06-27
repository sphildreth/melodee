namespace Melodee.Common.Models.Extensions;

public static class FileSystemFileInfoExtensions
{
    public static string FullName(this FileSystemFileInfo fileSystemFileInfo) => Path.Combine(fileSystemFileInfo.Path, fileSystemFileInfo.Name);

    public static bool Exists(this FileSystemFileInfo fileSystemFileInfo) => File.Exists(fileSystemFileInfo.FullName());

    public static string Extension(this FileSystemFileInfo fileSystemFileInfo) => Path.GetExtension(fileSystemFileInfo.FullName());
}