using System.Diagnostics;
using System.Text.RegularExpressions;
using ATL.Logging;
using Melodee.Common.Constants;
using Melodee.Common.Extensions;
using Melodee.Common.Utility;
using SearchOption = System.IO.SearchOption;

namespace Melodee.Common.Models.Extensions;

public static class FileSystemDirectoryInfoExtensions
{
    private static readonly Regex IsDirectoryNotStudioAlbumsRegex = new(@"(single(s)*|compilation(s*)|live|promo(s*)|demo)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static string FullName(this FileSystemDirectoryInfo fileSystemDirectoryInfo)
    {
        if (fileSystemDirectoryInfo.Path.EndsWith($"{ Path.DirectorySeparatorChar }{fileSystemDirectoryInfo.Name}"))
        {
            return fileSystemDirectoryInfo.Path;     
        }
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
            if (Crc32.Calculate(fileInfoForExtension) == crcHash)
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

    public static void DeleteAllEmptyDirectories(this FileSystemDirectoryInfo fileSystemDirectoryInfo)
    {
        DeleteEmptyDirs(fileSystemDirectoryInfo.FullName());
    }
    
    private static void DeleteEmptyDirs(string dir)
    {
        if (string.IsNullOrEmpty(dir))
        {
            throw new ArgumentException("Starting directory is a null reference or an empty string", nameof(dir));
        }

        if (!Directory.Exists(dir))
        {
            Trace.WriteLine($"Delete Empty Dirs called with a directory that does not exist [{dir}]", TraceLevel.Warning.ToString());
            return;
        }
        try
        {
            foreach (var d in Directory.EnumerateDirectories(dir))
            {
                DeleteEmptyDirs(d);
            }
            if (Directory.EnumerateFileSystemEntries(dir).Any())
            {
                return;
            }
            try
            {
                Directory.Delete(dir);
                Serilog.Log.Debug("\ud83d\udeae Deleted Empty Directory [{dir}]", dir);                
            }
            catch (UnauthorizedAccessException) { }
            catch (DirectoryNotFoundException) { }
        }
        catch (UnauthorizedAccessException) { }
    }      

    public static void DeleteAllFilesForExtension(this FileSystemDirectoryInfo fileSystemDirectoryInfo, string extension)
    {
        var filesToDelete = fileSystemDirectoryInfo.FileInfosForExtension(extension);
        foreach (var fileToDelete in filesToDelete)
        {
            fileToDelete.Delete();
        }
    }

    public static void MarkAllFilesForExtensionsSkipped(this FileSystemDirectoryInfo fileSystemDirectoryInfo, Dictionary<string, object?> configuration, params string[] extensions)
        => ChangeFileExtensions(fileSystemDirectoryInfo, SafeParser.ToString(configuration[SettingRegistry.ProcessingSkippedExtension]), extensions);
    
    public static void MarkAllFilesForExtensionsProcessed(this FileSystemDirectoryInfo fileSystemDirectoryInfo, Dictionary<string, object?> configuration, params string[] extensions)
        => ChangeFileExtensions(fileSystemDirectoryInfo, SafeParser.ToString(configuration[SettingRegistry.ProcessingProcessedExtension]), extensions);

    private static void ChangeFileExtensions(this FileSystemDirectoryInfo fileSystemDirectoryInfo, string newExtension, params string[] extensions)
    {
        if (newExtension.Nullify() == null)
        {
            return;
        }
        foreach (var extension in extensions)
        {
            var filesToMarkProcessed = fileSystemDirectoryInfo.FileInfosForExtension(extension);
            foreach (var fileToDelete in filesToMarkProcessed)
            {
                var moveToFileName = Path.Combine(fileToDelete.DirectoryName!, $"{fileToDelete.Name}.{ newExtension }");                
                fileToDelete.MoveTo(moveToFileName);
            }
        }
    }

    public static bool IsDirectoryNotStudioAlbums(this FileSystemDirectoryInfo fileSystemDirectoryInfo)
    {
        var dir = fileSystemDirectoryInfo.FullName();
        return !string.IsNullOrWhiteSpace(dir) && !IsDirectoryNotStudioAlbumsRegex.IsMatch(dir);
    }
}
