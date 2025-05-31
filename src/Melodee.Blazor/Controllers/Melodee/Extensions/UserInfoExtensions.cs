using Melodee.Blazor.Controllers.Melodee.Models;
using Melodee.Common.Configuration;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;


namespace Melodee.Blazor.Controllers.Melodee.Extensions;

public static class UserInfoExtensions
{
    public static User ToUserModel(this UserInfo user, string baseUrl)
    {
        return new User(user.ApiKey,
            $"{baseUrl}/images/{user.ToApiKey()}/{MelodeeConfiguration.DefaultThumbNailSize}",
            $"{baseUrl}/images/{user.ToApiKey()}/{MelodeeConfiguration.DefaultAvatarImageSize}",
            user.UserName,
            user.Email,
            false,
            false,
            [],
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            string.Empty,
            string.Empty            
        );
    }
}
