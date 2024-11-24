using Melodee.Common.Extensions;

namespace Melodee.Common.Models.Extensions;

public static class UserInfoExtensions
{
    public static string ToAvatarFilename(this UserInfo user, string libraryPath) => Path.Combine(libraryPath, $"{user.Id.ToStringPadLeft(8)}.png");
}
