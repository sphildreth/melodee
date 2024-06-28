using Melodee.Common.Models;

namespace Melodee.Common.Extensions;

public static class FileInfoExtensions
{
    public static FileSystemFileInfo ToFileSystemInfo(this System.IO.FileInfo fileInfo) => new FileSystemFileInfo
    {
        Path = fileInfo.DirectoryName ?? string.Empty,
        Name = fileInfo.Name,
        Size = fileInfo.Exists ? fileInfo.Length : 0
    };
}