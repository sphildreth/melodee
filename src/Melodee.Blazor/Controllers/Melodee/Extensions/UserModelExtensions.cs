using System.Globalization;
using Melodee.Common.Configuration;
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
}
