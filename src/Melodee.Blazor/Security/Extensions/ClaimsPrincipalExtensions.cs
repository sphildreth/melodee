using System.Globalization;
using System.Net;
using System.Security.Claims;
using Melodee.Common.Extensions;
using Melodee.Common.Constants;
using Melodee.Common.Data.Constants;
using NodaTime;

namespace Melodee.Blazor.Security.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static CultureInfo GetCulture(this ClaimsPrincipal principal)
        => CultureInfo.CurrentCulture;

    public static string? FormatNumber(this ClaimsPrincipal principal, short? number)
        => number?.ToStringPadLeft(5);
    
    public static string? FormatDateTime(this ClaimsPrincipal principal, Instant? dateTime)
        => dateTime?.ToString("yyyy-MM-dd HH:mm:ss", principal.GetCulture());
    
    public static bool IsAdmin(this ClaimsPrincipal principal)
        => principal.IsInRole(RoleNameRegistry.Administrator);

    public static string ToApiKey(this ClaimsPrincipal principal)
        => $"user{OpenSubsonicServer.ApiIdSeparator}{principal.FindFirstValue(ClaimTypes.Sid)}";
    
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
