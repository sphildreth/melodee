namespace Melodee.Common.Models.Extensions;

public static class FileInfoExtensions
{
    public static FileSystemFileInfo ToFileSystemInfo(this System.IO.FileInfo fileInfo) => new FileSystemFileInfo
    {
        Path = fileInfo.DirectoryName ?? string.Empty,
        Name = fileInfo.Name,
        Size = fileInfo.Length
    };
}