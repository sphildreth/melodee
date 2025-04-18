using Melodee.Common.Models;

namespace Melodee.Common.Extensions;

public static class DirectoryInfoExtensions
{
    public static bool IsSameDirectory(this DirectoryInfo directoryInfo, string path)
    {
        var di = directoryInfo.Name.TrimEnd('\\').TrimEnd('/');
        var p = path.TrimEnd('\\').TrimEnd('/');
        return string.Equals(di, p, StringComparison.OrdinalIgnoreCase);
    }
    
    public static FileInfo? FindFirstFileInParentDirectories(this DirectoryInfo startDirectory, string fileName)
    {
        DirectoryInfo? currentDirectory = startDirectory;
    
        // Continue checking parent directories until we reach the root or find the file
        while (currentDirectory != null)
        {
            // Check if the file exists in the current directory
            string filePath = Path.Combine(currentDirectory.FullName, fileName);
            if (File.Exists(filePath))
            {
                return new FileInfo(filePath);
            }
        
            // Move up to the parent directory
            currentDirectory = currentDirectory.Parent;
        }
    
        // Return null if no file was found
        return null;
    }    
    
    public static FileSystemDirectoryInfo? ToFileSystemDirectoryInfo(this DirectoryInfo? directoryInfo)
    {
        if (directoryInfo == null)
        {
            return null;
        }

        return new FileSystemDirectoryInfo
        {
            Path = directoryInfo.FullName,
            Name = directoryInfo.Name
        };
    }
}
