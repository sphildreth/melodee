using System.Globalization;
using System.Net;
using System.Security.Claims;
using Melodee.Common.Configuration;
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
        => number?.ToStringPadLeft(3) ?? MelodeeConfiguration.DefaultNoValuePlaceHolder;
    
    public static string? FormatNumber(this ClaimsPrincipal principal, int? number)
        => number?.ToStringPadLeft(5) ?? MelodeeConfiguration.DefaultNoValuePlaceHolder;    
    
    public static string? FormatInstant(this ClaimsPrincipal principal, Instant? instant)
        => instant?.ToString("yyyy-MM-dd HH:mm:ss", principal.GetCulture()) ?? MelodeeConfiguration.DefaultNoValuePlaceHolder;
    
    public static string? FormatDuration(this ClaimsPrincipal principal, Duration? duration)
        => duration?.ToString("-H", principal.GetCulture())?? MelodeeConfiguration.DefaultNoValuePlaceHolder;    
    
    public static bool IsAdmin(this ClaimsPrincipal principal)
        => principal.IsInRole(RoleNameRegistry.Administrator);

    public static string ToApiKey(this ClaimsPrincipal principal)
        => $"user{OpenSubsonicServer.ApiIdSeparator}{principal.FindFirstValue(ClaimTypes.Sid)}";
    
    public static bool IsEditor(this ClaimsPrincipal principal)
    {
        return principal.IsInRole(RoleNameRegistry.Editor) || principal.IsAdmin();
    }
}
