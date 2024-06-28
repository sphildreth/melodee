namespace Melodee.Common.Models.Extensions;

public static class FileSystemDirectoryInfoExtensions
{
   
    public static IEnumerable<System.IO.FileInfo> FileInfosForExtension(this FileSystemDirectoryInfo fileSystemDirectoryInfo, string extension)
    {
        var dirInfo = new System.IO.DirectoryInfo(fileSystemDirectoryInfo.Path);
        if (!dirInfo.Exists)
        {
            return [];
        }
        return dirInfo.GetFiles($"*.{extension}", SearchOption.AllDirectories).ToArray();
    }

    public static void DeleteAllFilesForExtension(this FileSystemDirectoryInfo fileSystemDirectoryInfo, string extension)
    {
        var filesToDelete = fileSystemDirectoryInfo.FileInfosForExtension(extension);
        foreach (var fileToDelete in filesToDelete)
        {
            fileToDelete.Delete();
        }
    }
    
  
}