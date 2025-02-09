using Melodee.Common.Models;

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
    
    public static bool CanWriteTo(this FileInfo fileInfo)
    {
        try
        {
            using (FileStream fs = fileInfo.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                return true;
            }
        }
        catch
        {
            return false;
        }
    }
}
