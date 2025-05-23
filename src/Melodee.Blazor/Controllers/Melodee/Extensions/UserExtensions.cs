using Melodee.Blazor.Controllers.Melodee.Models;
using Melodee.Common.Data.Models.Extensions;
using MelodeeDataModels = Melodee.Common.Data.Models;

namespace Melodee.Blazor.Controllers.Melodee.Extensions;

public static class UserExtensions
{
    public static Models.User ToUserModel(this MelodeeDataModels.User user, string baseUrl)
    {
        return new User(user.ApiKey,
            $"{baseUrl}/images/{user.ToApiKey()}/32",
            $"{baseUrl}/images/{user.ToApiKey()}/128",
            user.UserName,
            user.Email,
            user.IsAdmin,
            user.IsAdmin || user.IsEditor,
            user.Roles(),
            0,
            0,
            0,
            0,
            0,
            0,
            0);
    }
}
