using Melodee.Common.Utility;
using SearchOption = System.IO.SearchOption;

namespace Melodee.Common.Models.Extensions;

public static class FileSystemDirectoryInfoExtensions
{
    public static string FullName(this FileSystemDirectoryInfo fileSystemDirectoryInfo) => Path.Combine(fileSystemDirectoryInfo.Path, fileSystemDirectoryInfo.Name);

    public static IEnumerable<System.IO.FileInfo> FileInfosForExtension(this FileSystemDirectoryInfo fileSystemDirectoryInfo, string extension)
    {
        var dirInfo = new System.IO.DirectoryInfo(fileSystemDirectoryInfo.Path);
        if (!dirInfo.Exists)
        {
            return [];
        }
        return dirInfo.GetFiles($"*.{extension}", SearchOption.AllDirectories).ToArray();
    }

    public static IEnumerable<System.IO.FileInfo> AllFileImageTypeFileInfos(this FileSystemDirectoryInfo fileSystemDirectoryInfo)
    {
        var result = new List<System.IO.FileInfo>();
        var dirInfo = new System.IO.DirectoryInfo(fileSystemDirectoryInfo.Path);
        if (!dirInfo.Exists)
        {
            return result;
        }
        result.AddRange(dirInfo.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly)
            .Where(fileInfo => FileHelper.IsFileImageType(fileInfo.Extension)));
        return result;
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