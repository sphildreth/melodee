using Melodee.Common.Data.Constants;
using Melodee.Common.Models.Cards;

namespace Melodee.Common.Models.Collection.Extensions;

public static class AlbumDataInfoExtensions
{
    public static string ToApiKey(this AlbumDataInfo albumDataInfo)
        => $"album{OpenSubsonicServer.ApiIdSeparator}{albumDataInfo.ApiKey}";
}
