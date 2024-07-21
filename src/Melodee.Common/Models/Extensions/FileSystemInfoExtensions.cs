using System.Runtime.CompilerServices;
using Melodee.Common.Extensions;
using Microsoft.VisualBasic.FileIO;

namespace Melodee.Common.Models.Extensions;

public static class FileSystemInfoExtensions
{
    public static FileSystemDirectoryInfo ToDirectorySystemInfo(this System.IO.FileSystemInfo fileInfo)
    {
        var dir = new System.IO.DirectoryInfo(fileInfo.FullName);
        return new FileSystemDirectoryInfo
        {
            Path = dir.FullName,
            Name = dir.Name
        };
    }    
    
    public static FileSystemFileInfo ToFileSystemInfo(this System.IO.FileSystemInfo fileInfo)
    {
        var fi = new System.IO.FileInfo(fileInfo.FullName);
        return FileInfoExtensions.ToFileSystemInfo(fi);
    }
}