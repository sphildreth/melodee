namespace Melodee.Common.Models.Extensions;

public static class DirectoryInfoExtensions
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
}