using System.Net;
using System.Security.Claims;
using Melodee.Common.Constants;
using ServiceStack;

namespace Melodee.Blazor.Security.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static bool IsAdmin(this ClaimsPrincipal principal)
    {
        return principal.IsInRole(RoleNameRegistry.Administrator);
    }
    
    public static bool IsEditor(this ClaimsPrincipal principal)
    {
        return principal.IsInRole(RoleNameRegistry.Editor) || principal.IsAdmin();
    }
    
    public static string ApiRequestQuery(this ClaimsPrincipal principal)
    {
        var userName = WebUtility.UrlEncode(principal.FindFirstValue(ClaimTypes.Name));
        var userSalt = WebUtility.UrlEncode(principal.FindFirstValue(ClaimTypeRegistry.UserSalt));
        var userToken = WebUtility.UrlEncode(principal.FindFirstValue(ClaimTypeRegistry.UserToken));
        return $"u={userName}&s={userSalt}&t={userToken}";
    }
}
