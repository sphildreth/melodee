namespace Melodee.Common.Utility;

public static class FileHelper
{
    public static readonly string MelodeeTagFileExtension = ".mtg";

    private static readonly IEnumerable<string> MediaMetaDataFileTypeExtensions =
    [
        "cue",
        "m3u",
        "sfv"
    ];

    private static readonly IEnumerable<string> MediaFileTypeExtensions =
    [
        "aac",
        "alac",
        "ac3",
        "bonk",
        "aiff",
        "ape",
        "asf",
        "flac",
        "m4a",
        "m4b",
        "mp1",
        "mp2",
        "mp3",
        "mp4",
        "oga",
        "ogg",
        "opus",
        "spx",
        "sfu",
        "svg",
        "tta",
        "wav",
        "wma"
    ];

    private static readonly IEnumerable<string> ImageFileTypeExtensions =
    [
        "bmp",
        "gif",
        "jfif",
        "image", // This is a temporary file extension used when downloading images and converting
        "jpeg",
        "jpg",
        "png",
        "tiff",
        "webp"
    ];

    public static bool IsFileMediaType(string? extension)
    {
        return !string.IsNullOrEmpty(extension) &&
               MediaFileTypeExtensions.Contains(extension.Replace(".", ""), StringComparer.OrdinalIgnoreCase);
    }

    public static bool IsFileImageType(string? extension)
    {
        return !string.IsNullOrEmpty(extension) &&
               ImageFileTypeExtensions.Contains(extension.Replace(".", ""), StringComparer.OrdinalIgnoreCase);
    }

    public static bool IsFileMediaMetaDataType(string? extension)
    {
        return !string.IsNullOrEmpty(extension) &&
               MediaMetaDataFileTypeExtensions.Contains(extension.Replace(".", ""),
                   StringComparer.OrdinalIgnoreCase);
    }

    public static int GetNumberOfTotalFilesForDirectory(DirectoryInfo directoryInfo)
    {
        return directoryInfo.EnumerateFiles("*.*", SearchOption.AllDirectories).Count();
    }

    public static IEnumerable<IGrouping<string, FileInfo>> AllFileExtensionsForDirectory(DirectoryInfo directoryInfo)
    {
        return directoryInfo.EnumerateFiles("*.*", SearchOption.AllDirectories).GroupBy(x => x.Extension);
    }

    public static int GetNumberOfMediaFilesForDirectory(IEnumerable<IGrouping<string, FileInfo>> fileExtensions)
    {
        var result = 0;

        foreach (var extGroup in fileExtensions)
        {
            if (IsFileMediaType(extGroup.Key))
            {
                result += extGroup.Count();
            }
        }

        return result;
    }

    public static int GetNumberOfImageFilesForDirectory(IEnumerable<IGrouping<string, FileInfo>> fileExtensions)
    {
        var result = 0;
        foreach (var extGroup in fileExtensions)
        {
            if (IsFileImageType(extGroup.Key))
            {
                result += extGroup.Count();
            }
        }

        return result;
    }

    public static int GetNumberOfMediaMetaDataFilesForDirectory(IEnumerable<IGrouping<string, FileInfo>> fileExtensions)
    {
        var result = 0;
        foreach (var extGroup in fileExtensions)
        {
            if (IsFileMediaMetaDataType(extGroup.Key))
            {
                result += extGroup.Count();
            }
        }

        return result;
    }
}
