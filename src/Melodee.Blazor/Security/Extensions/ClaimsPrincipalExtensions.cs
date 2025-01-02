using System.Net;
using System.Security.Claims;
using Melodee.Common.Constants;

namespace Melodee.Blazor.Security.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string ApiRequestQuery(this ClaimsPrincipal principal)
    {
        var userName = WebUtility.UrlEncode(principal.FindFirstValue(ClaimTypes.Name));
        var userSalt = WebUtility.UrlEncode(principal.FindFirstValue(ClaimTypeRegistry.UserSalt));
        var userToken = WebUtility.UrlEncode(principal.FindFirstValue(ClaimTypeRegistry.UserToken));
        return $"u={userName}&s={userSalt}&t={userToken}";
    }
}
