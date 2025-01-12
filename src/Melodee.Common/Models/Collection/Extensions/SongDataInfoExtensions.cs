using Melodee.Common.Data.Constants;

namespace Melodee.Common.Models.Collection.Extensions;

public static class SongDataInfoExtensions
{
    public static string ImageUrl(this SongDataInfo songDataInfo, int? size = null)
    {
        return $"/images/{songDataInfo.ToApiKey()}/{size ?? 80}";
    }

    public static string ToApiKey(this SongDataInfo songDataInfo)
    {
        return $"song{OpenSubsonicServer.ApiIdSeparator}{songDataInfo.ApiKey}";
    }
}
