using System.Web;
using Melodee.Common.Data.Constants;

namespace Melodee.Common.Models.Collection.Extensions;

public static class AlbumDataInfoExtensions
{
    public static string? ImageBase64(this AlbumDataInfo albumDataInfo, byte[] defaultImages)
    {
        return $"data:image/jpeg;base64,{Convert.ToBase64String(albumDataInfo.ImageBytes ?? defaultImages)}";
    }

    public static string ImageUrl(this AlbumDataInfo artistDataInfo, int? size = null)
    {
        return $"/images/{artistDataInfo.ToApiKey()}/{size ?? 80}";
    }

    public static string DetailUrl(this AlbumDataInfo albumDataInfo, string? fromUrl = null)
    {
        var result = $"/data/album/{albumDataInfo.ApiKey}";
        return fromUrl != null ? $"{result}/{HttpUtility.UrlEncode(fromUrl)}" : result;
    }

    public static string ArtistDetailUrl(this AlbumDataInfo albumDataInfo)
    {
        return $"/data/artist/{albumDataInfo.ArtistApiKey}";
    }

    public static string ToApiKey(this AlbumDataInfo albumDataInfo)
    {
        return $"album{OpenSubsonicServer.ApiIdSeparator}{albumDataInfo.ApiKey}";
    }
}
