using Melodee.Common.Data.Constants;
using Melodee.Common.Enums;

namespace Melodee.Common.Models.Collection.Extensions;

public static class ArtistDataInfoExtensions
{
    public static string? ImageBase64(this ArtistDataInfo artist, byte[]? defaultImages = null)
    {
        if (artist.ImageBytes == null && defaultImages == null)
        {
            return null;
        }

        return $"data:image/jpeg;base64,{Convert.ToBase64String(artist.ImageBytes ?? defaultImages ?? [])}";
    }

    public static FileSystemDirectoryInfo ToFileSystemDirectoryInfo(this ArtistDataInfo artist,
        string? libraryPath = null)
    {
        return new FileSystemDirectoryInfo
        {
            Path = libraryPath ?? artist.LibraryPath,
            Name = artist.Directory
        };
    }

    public static string DetailUrl(this ArtistDataInfo artistDataInfo)
    {
        return $"/data/artist/{artistDataInfo.ApiKey}";
    }

    public static string ImageUrl(this ArtistDataInfo artistDataInfo, ImageSize? size = null)
    {
        if (artistDataInfo.ImageBase64() != null)
        {
            return artistDataInfo.ImageBase64()!;
        }

        return $"/images/{artistDataInfo.ToApiKey()}/{size ?? ImageSize.Thumbnail}";
    }

    public static string ToApiKey(this ArtistDataInfo artist)
    {
        return $"artist{OpenSubsonicServer.ApiIdSeparator}{artist.ApiKey}";
    }
}
