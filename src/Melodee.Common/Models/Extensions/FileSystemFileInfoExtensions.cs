namespace Melodee.Common.Models.Extensions;

public static class FileSystemFileInfoExtensions
{
    
    public static string FullName(this FileSystemFileInfo fileSystemFileInfo, FileSystemDirectoryInfo directoryInfo) => Path.Combine(directoryInfo.Path, fileSystemFileInfo.Name);

    public static bool Exists(this FileSystemFileInfo fileSystemFileInfo, FileSystemDirectoryInfo directoryInfo) => File.Exists(fileSystemFileInfo.FullName(directoryInfo));

    public static string Extension(this FileSystemFileInfo fileSystemFileInfo, FileSystemDirectoryInfo directoryInfo) => Path.GetExtension(fileSystemFileInfo.FullName(directoryInfo));
}