using System.Text.RegularExpressions;
using Melodee.Common.Utility;
using SearchOption = System.IO.SearchOption;

namespace Melodee.Common.Models.Extensions;

public static class FileSystemDirectoryInfoExtensions
{
    private static readonly Regex IsDirectoryNotStudioAlbumsRegex = new(@"(single(s)*|compilation(s*)|live|promo(s*)|demo)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static string FullName(this FileSystemDirectoryInfo fileSystemDirectoryInfo)
    {
        return Path.Combine(fileSystemDirectoryInfo.Path, fileSystemDirectoryInfo.Name);
    }

    public static void EnsureExists(this FileSystemDirectoryInfo fileSystemDirectoryInfo)
    {
        if (!fileSystemDirectoryInfo.Exists())
        {
            Directory.CreateDirectory(fileSystemDirectoryInfo.FullName());
        }
    }
    
    public static bool Exists(this FileSystemDirectoryInfo fileSystemDirectoryInfo)
    {
        return Directory.Exists(fileSystemDirectoryInfo.FullName());
    }    

    public static IEnumerable<FileInfo> FileInfosForExtension(this FileSystemDirectoryInfo fileSystemDirectoryInfo, string extension)
    {
        var dirInfo = new DirectoryInfo(fileSystemDirectoryInfo.Path);
        if (!dirInfo.Exists)
        {
            return [];
        }

        return dirInfo.GetFiles($"*.{extension}", new EnumerationOptions
        {
            RecurseSubdirectories = true,
            MatchCasing = MatchCasing.CaseInsensitive
        }).OrderBy(x => x.Name).ToArray();
    }

    public static FileSystemFileInfo? GetFileForCrcHash(this FileSystemDirectoryInfo fileSystemDirectoryInfo, string extension, string crcHash)
    {
        foreach (var fileInfoForExtension in fileSystemDirectoryInfo.FileInfosForExtension(extension))
        {
            if (CRC32.Calculate(fileInfoForExtension) == crcHash)
            {
                return fileInfoForExtension.ToFileSystemInfo();
            }
        }

        return null;
    }

    public static IEnumerable<FileSystemDirectoryInfo> GetFileSystemDirectoryInfosToProcess(this FileSystemDirectoryInfo fileSystemDirectoryInfo, SearchOption searchOption)
    {
        if (string.IsNullOrWhiteSpace(fileSystemDirectoryInfo.Path))
        {
            return [];
        }

        var dirInfo = new DirectoryInfo(fileSystemDirectoryInfo.Path);
        if (!dirInfo.Exists)
        {
            return [];
        }

        var result = new List<FileSystemDirectoryInfo>();
        result.AddRange(from dir in dirInfo.EnumerateDirectories("*.*", searchOption) where dir.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly).Any(x => FileHelper.IsFileMediaType(x.Extension)) select dir.ToDirectorySystemInfo());
        if (dirInfo.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly).Any(x => FileHelper.IsFileMediaType(x.Extension)))
        {
            result.Add(fileSystemDirectoryInfo);
        }

        return result;
    }

    public static IEnumerable<FileInfo> AllFileImageTypeFileInfos(this FileSystemDirectoryInfo fileSystemDirectoryInfo)
    {
        var result = new List<FileInfo>();
        var dirInfo = new DirectoryInfo(fileSystemDirectoryInfo.Path);
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

    public static bool IsDirectoryNotStudioAlbums(this FileSystemDirectoryInfo fileSystemDirectoryInfo)
    {
        var dir = fileSystemDirectoryInfo.FullName();
        return !string.IsNullOrWhiteSpace(dir) && !IsDirectoryNotStudioAlbumsRegex.IsMatch(dir);
    }
}
