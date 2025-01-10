using Melodee.Common.Data.Constants;

namespace Melodee.Common.Models.Collection.Extensions;

public static class SongDataInfoExtensions
{
    public static string ImageUrl(this SongDataInfo songDataInfo, int? size = null)
        => $"/images/{songDataInfo.ToApiKey()}/{ size ?? 80}";
    
    public static string ToApiKey(this SongDataInfo songDataInfo)
        => $"song{OpenSubsonicServer.ApiIdSeparator}{songDataInfo.ApiKey}";
}
