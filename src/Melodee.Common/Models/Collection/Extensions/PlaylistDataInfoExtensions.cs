using System.Web;
using Melodee.Common.Data.Constants;

namespace Melodee.Common.Models.Collection.Extensions;

public static class PlaylistDataInfoExtensions
{
    public static string ToApiKey(this PlaylistDataInfo playlistDataInfo)
    {
        return $"playlist{OpenSubsonicServer.ApiIdSeparator}{playlistDataInfo.ApiKey}";
    }
}
