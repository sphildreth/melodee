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
    
    /// <summary>
    /// This exists because in some systems where data is on one mapped drive it cannot be "Moved" to another mapped drive ("Cross link" error), it must be copied and then deleted.
    /// </summary>
    public static void MoveFile(this FileSystemFileInfo fileSystemFileInfo, FileSystemDirectoryInfo directoryInfo, string destinationFileName)
    {
        var fullName = fileSystemFileInfo.FullOriginalName(directoryInfo);        
        if (fileSystemFileInfo.Exists(directoryInfo))
        {
            File.Copy(fullName, destinationFileName);
            File.Delete(fullName);
            return;
        }
        Log.Warning("Unable to move file [{Filename}]. File not found.", fullName);
    }     
}
