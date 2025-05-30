using System.Globalization;
using System.Web;
using Melodee.Common.Configuration;
using Melodee.Common.Extensions;
using Melodee.Common.Security;
using NodaTime;

namespace Melodee.Blazor.Controllers.Melodee.Extensions;

public static class UserModelExtensions
{
    public static CultureInfo GetCulture(this Models.User user)
    {
        return CultureInfo.CurrentCulture;
    }

    public static string FormatDuration(this Models.User user, Duration duration)
    {
        return duration.ToString("-H:mm:ss", user.GetCulture());
    }
    
    public static string FormatInstant(this Models.User user, Instant? instant)
    {
        return instant?.ToString("yyyy-MM-dd HH:mm:ss", user.GetCulture()) ?? MelodeeConfiguration.DefaultNoValuePlaceHolder;
    }    

    public static string CreateAuthUrlFragment(this Models.User user, string secret, string seed)
    {
        var hmacService = new HmacTokenService(secret);
        return HttpUtility.UrlEncode(hmacService.GenerateTimedToken($"{user.Id}:{seed}").ToBase64());
    }
}
