using System.Text.RegularExpressions;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;

namespace Melodee.Common.Utility;

public static partial class ImageHelper
{
    private static string[] ArtistImageFileNames =>
    [
        "BAND",
        "ARTIST",
        "GROUP",
        "PHOTO"
    ];

    private static string[] ArtistSecondaryImageFileNames =>
    [
        "ARTISTLOGO",
        "LOGO"
    ];

    private static string[] AlbumImageFileNames =>
    [
        "ALBUM",
        "ART",
        "BIG",
        "COVER",
        "COVERS",
        "CVR",
        "FOLDER",
        "FRONT",
        "SCAN",
        "SCANS"
    ];

    private static string[] AlbumSecondaryImageFileNames =>
    [
        "BACK",
        "BOOK",
        "CD",
        "DIGIPACK",
        "DISC",
        "DVD",
        "ENCARTES",
        "INSIDE",
        "INNER",
        "INLAY",
        "INSITE",
        "JEWEL",
        "MATRIX",
        "TRAYCARD"
    ];

    private static string[] ImageExtensions()
    {
        return ["*.bmp", "*.jpeg", "*.jpe", "*.jpg", "*.png", "*.gif", "*.webp"];
    }

    public static string[]? NormalizedImageTypesFromFilename(string fileName)
    {
        string[] filePartsToMatch;

        var fn = Path.GetFileNameWithoutExtension(fileName);
        if (ImageFileNameSeperatorsRegex().IsMatch(fileName))
        {
            filePartsToMatch = fn.Replace('-', ' ').Replace('_', ' ').Split(' ');
        }
        else
        {
            filePartsToMatch = [fn];
        }

        if (filePartsToMatch.Length > 0)
        {
            var result = new List<string>();
            foreach (var filePartToMatch in filePartsToMatch)
            {
                var n = filePartToMatch.ToNormalizedString() ?? filePartToMatch;
                if (ArtistImageFileNames.Any(x => x == (n.ToNormalizedString() ?? n)))
                {
                    result.Add(n.ToNormalizedString() ?? n);
                }

                if (AlbumImageFileNames.Any(x => x == (n.ToNormalizedString() ?? n)))
                {
                    result.Add(n.ToNormalizedString() ?? n);
                }
            }

            return result.Count > 0 ? result.ToArray() : null;
        }

        return null;
    }

    private static string[] GetFiles(string? path,
        string[]? patterns = null,
        SearchOption options = SearchOption.TopDirectoryOnly)
    {
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            return [];
        }

        if (patterns == null || patterns.Length == 0)
        {
            return Directory.GetFiles(path, "*", options);
        }

        if (patterns.Length == 1)
        {
            return Directory.GetFiles(path, patterns[0], options);
        }

        return patterns.SelectMany(pattern => Directory.GetFiles(path, pattern, options)).Distinct().ToArray();
    }

    public static string[] ImageFilesInDirectory(string? directory, SearchOption searchOption)
    {
        return GetFiles(directory, ImageExtensions(), searchOption);
    }

    public static bool IsArtistImage(FileInfo? fileInfo)
    {
        if (fileInfo == null)
        {
            return false;
        }

        if (FileHelper.IsFileImageType(fileInfo.Extension))
        {
            var normalizedName = fileInfo.Name.ToNormalizedString() ?? fileInfo.Name;
            if (ArtistImageFileNames.Any(artistImage => normalizedName.Contains(artistImage)))
            {
                var nameDigits = string.Join(string.Empty, fileInfo.Name.Where(char.IsDigit));
                return SafeParser.ToNumber<int>(nameDigits) < 2;
            }
        }

        return false;
    }

    public static bool IsArtistSecondaryImage(FileInfo? fileInfo)
    {
        if (fileInfo == null)
        {
            return false;
        }

        if (FileHelper.IsFileImageType(fileInfo.Extension))
        {
            var normalizedName = fileInfo.Name.ToNormalizedString() ?? fileInfo.Name;
            if (ArtistImageFileNames.Any(artistImage => normalizedName.Contains(artistImage)))
            {
                var nameDigits = string.Join(string.Empty, fileInfo.Name.Where(char.IsDigit));
                return SafeParser.ToNumber<int>(nameDigits) > 1;
            }

            if (ArtistSecondaryImageFileNames.Any(artistImage => normalizedName.Contains(artistImage)))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsAlbumImage(FileInfo? fileInfo)
    {
        if (fileInfo == null)
        {
            return false;
        }

        if (FileHelper.IsFileImageType(fileInfo.Extension))
        {
            var normalizedName = Path.GetFileNameWithoutExtension(fileInfo.Name).ToNormalizedString() ?? fileInfo.Name;
            if (AlbumSecondaryImageFileNames.Any(artistImage => normalizedName.Contains(artistImage)))
            {
                return false;
            }

            var nameDigits = string.Join(string.Empty, fileInfo.Name.Where(char.IsDigit)).Nullify();
            var numberInName = SafeParser.ToNumber<int>(nameDigits);
            var nt = NormalizedImageTypesFromFilename(fileInfo.Name);
            var ntMatches = AlbumImageFileNames.Count(x => nt?.Contains(x) ?? false);

            if (ntMatches > 0)
            {
                // Primary image is 00 and 01, sometimes images have year in them, so if 00 or 01 or greater than minimum year
                return numberInName is > 1860 or < 2;
            }
        }

        return false;
    }

    public static bool IsAlbumSecondaryImage(FileInfo? fileInfo)
    {
        if (fileInfo == null)
        {
            return false;
        }

        if (IsAlbumImage(fileInfo))
        {
            return false;
        }

        if (FileHelper.IsFileImageType(fileInfo.Extension))
        {
            var normalizedName = fileInfo.Name.ToNormalizedString() ?? fileInfo.Name;
            if (AlbumSecondaryImageFileNames.Any(artistImage => normalizedName.Contains(artistImage)))
            {
                return true;
            }

            if (AlbumImageFileNames.Any(artistImage => normalizedName.Contains(artistImage)))
            {
                var nameDigits = string.Join(string.Empty, fileInfo.Name.Where(char.IsDigit));
                return SafeParser.ToNumber<int>(nameDigits) > 1;
            }
        }

        return false;
    }


    public static IEnumerable<FileInfo> FindImageTypeInDirectory(
        DirectoryInfo? directory,
        PictureIdentifier type,
        SearchOption directorySearchOptions = SearchOption.AllDirectories)
    {
        var result = new List<FileInfo>();
        if (directory?.Exists != true)
        {
            return result;
        }

        var imageFilesInDirectory = ImageFilesInDirectory(directory.FullName, directorySearchOptions);
        if (imageFilesInDirectory.Any() != true)
        {
            return result;
        }

        foreach (var imageFile in imageFilesInDirectory)
        {
            var image = new FileInfo(imageFile);
            switch (type)
            {
                case PictureIdentifier.Artist:
                    if (IsArtistImage(image))
                    {
                        result.Add(image);
                    }

                    break;

                case PictureIdentifier.ArtistSecondary:
                    if (IsArtistSecondaryImage(image))
                    {
                        result.Add(image);
                    }

                    break;

                case PictureIdentifier.Front:
                    if (IsAlbumImage(image))
                    {
                        result.Add(image);
                    }

                    if (IsAlbumSecondaryImage(image))
                    {
                        result.Add(image);
                    }

                    break;
            }
        }

        return result.OrderBy(x => x.Name);
    }

    [GeneratedRegex(@".*(-| |_)+.*", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex ImageFileNameSeperatorsRegex();
}
