using Melodee.Common.Models;
using Serilog;

namespace Melodee.Common.Extensions;

public static class FileInfoExtensions
{
    public static FileSystemFileInfo ToFileSystemInfo(this FileInfo fileInfo)
    {
        return new FileSystemFileInfo
        {
            Name = fileInfo.Name,
            Size = fileInfo.Exists ? fileInfo.Length : 0,
            OriginalName = fileInfo.Name
        };
    }
}
