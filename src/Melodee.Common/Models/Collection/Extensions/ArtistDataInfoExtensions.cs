using Melodee.Common.Data.Constants;

namespace Melodee.Common.Models.Collection.Extensions;

public static class ArtistDataInfoExtensions
{
    public static FileSystemDirectoryInfo ToFileSystemDirectoryInfo(this ArtistDataInfo artist, string? libraryPath = null)
    {
        return new FileSystemDirectoryInfo
        {
            Path = libraryPath ?? artist.LibraryPath,
            Name = artist.Directory
        };
    }

    public static string ImageUrl(this ArtistDataInfo artistDataInfo, int? size = null)
        => $"/images/{artistDataInfo.ToApiKey()}/{ size ?? 80}";
    
    public static string ToApiKey(this ArtistDataInfo artist)
        => $"artist{OpenSubsonicServer.ApiIdSeparator}{artist.ApiKey}";
}
