using Melodee.Common.Data.Constants;

namespace Melodee.Common.Models.Collection.Extensions;

public static class AlbumDataInfoExtensions
{
    public static string? ImageBase64(this AlbumDataInfo albumDataInfo)
        => albumDataInfo.ImageBytes == null ? null : $"data:image/jpeg;base64,{Convert.ToBase64String(albumDataInfo.ImageBytes)}";
    
    public static string ImageUrl(this AlbumDataInfo artistDataInfo, int? size = null)
        => $"/images/{artistDataInfo.ToApiKey()}/{ size ?? 80}";
    
    public static string ToApiKey(this AlbumDataInfo albumDataInfo)
        => $"album{OpenSubsonicServer.ApiIdSeparator}{albumDataInfo.ApiKey}";
}