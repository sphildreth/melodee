using Melodee.Common.Data.Constants;
using Melodee.Common.Extensions;

namespace Melodee.Common.Models.Collection.Extensions;

public static class UserDataInfoExtensions
{
    public static string ToAvatarFileName(this UserDataInfo user, string libraryPath)
    {
        return Path.Combine(libraryPath, $"{user.Id.ToStringPadLeft(8)}.gif");
    }

    public static string ToApiKey(this UserDataInfo user)
    {
        return $"user{OpenSubsonicServer.ApiIdSeparator}{user.ApiKey}";
    }
}
