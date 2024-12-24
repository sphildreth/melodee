using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using Commons;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Utility;
using NodaTime;
using Serilog;
using SearchOption = System.IO.SearchOption;

namespace Melodee.Common.Models.Extensions;

public static class FileSystemDirectoryInfoExtensions
{
    public static readonly Regex IsDirectoryNotStudioAlbumsRegex = new(@"(single(s)*|\s?best\s?of|greatest(s*)\s?hit(s*)|compilation(s*)|live|boxset(s*)|bootleg(s*)|promo(s*)|demo(s*))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static readonly Regex IsDirectoryDiscographyRegex = new("(discography)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    public static readonly Regex IsDirectoryAlbumMediaDirectoryRegex = new("(cd|disc|disk|side|media|a|b|c|d|e|f)([0-9]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Rename the Directory and prepend the given prefix.
    /// </summary>
    public static void AppendPrefix(this FileSystemDirectoryInfo fileSystemDirectoryInfo, string prefix)
    {
        var newDir = Path.Combine(prefix, fileSystemDirectoryInfo.FullName());
        if (newDir != fileSystemDirectoryInfo.FullName())
        {
            fileSystemDirectoryInfo.ToDirectoryInfo().MoveTo(newDir);
        }
    }

    /// <summary>
    /// Rename the Directory and append the given suffix.
    /// </summary>
    public static void AppendSuffix(this FileSystemDirectoryInfo fileSystemDirectoryInfo, string suffix)
    {
        fileSystemDirectoryInfo.ToDirectoryInfo().MoveTo(Path.Combine(fileSystemDirectoryInfo.FullName(), suffix));
    }
    
    public static DirectoryInfo ToDirectoryInfo(this FileSystemDirectoryInfo fileSystemDirectoryInfo)
    {
        return new DirectoryInfo(fileSystemDirectoryInfo.FullName());
    }

    public static string FullName(this FileSystemDirectoryInfo fileSystemDirectoryInfo)
    {
        var p = fileSystemDirectoryInfo.Path;
        if (p.LastOrDefault() == Path.DirectorySeparatorChar)
        {
            p = p[..^1];
        }

        if (p.EndsWith($"{Path.DirectorySeparatorChar}{fileSystemDirectoryInfo.Name}"))
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

    public static bool IsDiscographyDirectory(this FileSystemDirectoryInfo fileSystemDirectoryInfo)
    {
        var dir = fileSystemDirectoryInfo.Name;
        return !string.IsNullOrWhiteSpace(dir) && IsDirectoryDiscographyRegex.IsMatch(dir);
    }

    public static bool IsAlbumMediaDirectory(this FileSystemDirectoryInfo fileSystemDirectoryInfo)
    {
        var dir = fileSystemDirectoryInfo.Name;
        return !string.IsNullOrWhiteSpace(dir) && IsDirectoryAlbumMediaDirectoryRegex.IsMatch(dir);
    }

    public static IEnumerable<FileSystemDirectoryInfo> AllAlbumMediaDirectories(this FileSystemDirectoryInfo fileSystemDirectoryInfo)
    {
        var result = new List<FileSystemDirectoryInfo>();
        foreach (var dirInfo in fileSystemDirectoryInfo.AllDirectoryInfos())
        {
            if (dirInfo.ToDirectorySystemInfo().IsAlbumMediaDirectory())
            {
                result.Add(dirInfo.ToDirectorySystemInfo());
            }
        }
        return result.ToArray();
    }

    public static FileSystemDirectoryInfo GetParent(this FileSystemDirectoryInfo fileSystemDirectoryInfo)
        => fileSystemDirectoryInfo.GetParents().First();

    public static IEnumerable<FileSystemDirectoryInfo> GetParents(this FileSystemDirectoryInfo fileSystemDirectoryInfo)
    {
        var results = new List<FileSystemDirectoryInfo>();
        var dirInfo = fileSystemDirectoryInfo.ToDirectoryInfo();
        var parent = dirInfo.Parent;
        while (parent != null)
        {
            results.Add(parent.ToDirectorySystemInfo());
            parent = parent.Parent;
        }
        return results.ToArray();
    }

    public static IEnumerable<FileSystemDirectoryInfo> GetFileSystemDirectoryInfosToProcess(this FileSystemDirectoryInfo fileSystemDirectoryInfo, IMelodeeConfiguration configuration, Instant? modifiedSince, SearchOption searchOption)
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
        var modifiedSinceValue = modifiedSince?.ToDateTimeUtc() ?? DateTime.MinValue;
        result.AddRange(from dir in dirInfo.EnumerateDirectories("*.*", searchOption).OrderBy(x => x.LastWriteTimeUtc) where dir.LastWriteTimeUtc >= modifiedSinceValue && dir.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly).Any(x => FileHelper.IsFileMediaType(x.Extension)) select dir.ToDirectorySystemInfo());
        if (dirInfo.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly)
            .Any(x => x.LastWriteTimeUtc >= modifiedSinceValue && FileHelper.IsFileMediaType(x.Extension)))
        {
            result.Add(fileSystemDirectoryInfo);
        }
        var skipDirPrefix = configuration.GetValue<string>(SettingRegistry.ProcessingSkippedDirectoryPrefix);
        if (skipDirPrefix.Nullify() != null)
        {
            return result.Where(x => !x.Name.StartsWith(skipDirPrefix!)).ToArray();
        }
        return result.ToArray();
    }

    public static (string, int) GetNextFileNameForType(this FileSystemDirectoryInfo fileSystemDirectoryInfo, short maximumNumberOfImageTypeAllowed, string imageType)
    {
        var highestNumberFound = 0;
        var maxNumberLength = SafeParser.ToNumber<short>(maximumNumberOfImageTypeAllowed.ToString().Length)+1;
        var allImagesInDirectory = fileSystemDirectoryInfo.AllFileImageTypeFileInfos().ToArray();
        if (allImagesInDirectory.Length != 0)
        {
            foreach (var image in allImagesInDirectory)
            {
                if (image.Name.EndsWith($"{imageType}.jpg", StringComparison.OrdinalIgnoreCase))
                {
                    var number = SafeParser.ToNumber<short>(image.Name.Substring(ImageInfo.ImageFilePrefix.Length, maxNumberLength));
                    if (number > highestNumberFound)
                    {
                        highestNumberFound = number;
                    }
                }
            }
        }
        highestNumberFound++;
        return ($"{ImageInfo.ImageFilePrefix}{highestNumberFound.ToStringPadLeft(maximumNumberOfImageTypeAllowed)}-{imageType}.jpg", highestNumberFound);
    }

    public static IEnumerable<FileInfo> AllFileImageTypeFileInfos(this FileSystemDirectoryInfo fileSystemDirectoryInfo)
    {
        return fileSystemDirectoryInfo.AllFileInfos().Where(fileInfo => FileHelper.IsFileImageType(fileInfo.Extension));
    }
    
    public static IEnumerable<FileInfo> AllMediaTypeFileInfos(this FileSystemDirectoryInfo fileSystemDirectoryInfo)
    {
        return fileSystemDirectoryInfo.AllFileInfos().Where(fileInfo => FileHelper.IsFileMediaType(fileInfo.Extension));
    }    

    public static IEnumerable<FileInfo> AllFileInfos(this FileSystemDirectoryInfo fileSystemDirectoryInfo, string? searchPattern = null)
    {
        var dirInfo = new DirectoryInfo(fileSystemDirectoryInfo.Path);
        if (!dirInfo.Exists)
        {
            return [];
        }

        return dirInfo.EnumerateFiles(searchPattern ?? "*.*", SearchOption.TopDirectoryOnly);
    }
    
    public static IEnumerable<DirectoryInfo> AllDirectoryInfos(this FileSystemDirectoryInfo fileSystemDirectoryInfo, string? searchPattern = null)
    {
        var dirInfo = new DirectoryInfo(fileSystemDirectoryInfo.Path);
        if (!dirInfo.Exists)
        {
            return [];
        }
        return dirInfo.EnumerateDirectories(searchPattern ?? "*.*", SearchOption.TopDirectoryOnly);
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
                Log.Debug("\ud83d\udeae Deleted Empty Directory [{dir}]", dir);
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (DirectoryNotFoundException)
            {
            }
        }
        catch (UnauthorizedAccessException)
        {
        }
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
    {
        ChangeFileExtensions(fileSystemDirectoryInfo, SafeParser.ToString(configuration[SettingRegistry.ProcessingSkippedExtension]), extensions);
    }

    public static void MarkAllFilesForExtensionsProcessed(this FileSystemDirectoryInfo fileSystemDirectoryInfo, Dictionary<string, object?> configuration, params string[] extensions)
    {
        ChangeFileExtensions(fileSystemDirectoryInfo, SafeParser.ToString(configuration[SettingRegistry.ProcessingProcessedExtension]), extensions);
    }

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
                var moveToFileName = Path.Combine(fileToDelete.DirectoryName!, $"{fileToDelete.Name}.{newExtension}");
                fileToDelete.MoveTo(moveToFileName);
            }
        }
    }

    /// <summary>
    /// This is false then the name of the directory has fragments that test for non studio albums (e.g. 'live', 'compilation')
    /// </summary>
    /// <param name="fileSystemDirectoryInfo"></param>
    /// <returns></returns>
    public static bool IsDirectoryStudioAlbums(this FileSystemDirectoryInfo fileSystemDirectoryInfo)
    {
        var dir = fileSystemDirectoryInfo.FullName();
        return !string.IsNullOrWhiteSpace(dir) && !IsDirectoryNotStudioAlbumsRegex.IsMatch(dir);
    }
}
