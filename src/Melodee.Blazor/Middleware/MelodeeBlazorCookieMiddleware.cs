using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Utility;

namespace Melodee.Blazor.Middleware;

public class MelodeeBlazorCookieMiddleware(RequestDelegate next, IMelodeeConfigurationFactory configurationFactory)
{
    public const string DateFormat = "yyyyMMdd";
    public const string CookieName = "melodee_blazor_token";

    public async Task InvokeAsync(HttpContext context)
    {
        var isApiCall = context.Request.Path.StartsWithSegments("/api");
        if (isApiCall)
        {
            await next(context);
            return;
        }
        var configuration = await configurationFactory.GetConfigurationAsync();
        context.Response.Cookies.Append(CookieName,
            HashHelper.CreateMd5(DateTime.UtcNow.ToString(DateFormat) + configuration.GetValue<string>(SettingRegistry.EncryptionPrivateKey)) ?? string.Empty,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.None, // this is none as some players are hosted and cross site to Melodee servers (e.g. Feishin on vercel)
                Expires = DateTime.UtcNow.AddDays(1)
            });
        await next(context);
    }


    public static async Task<bool> ValidateCookie(string? cookie, IMelodeeConfigurationFactory configurationFactory)
    {
        if (cookie is null)
        {
            return false;
        }

        var configuration = await configurationFactory.GetConfigurationAsync();
        return HashHelper.CreateMd5(DateTime.UtcNow.ToString("yyyyMMdd") + configuration.GetValue<string>(SettingRegistry.EncryptionPrivateKey)) == cookie;
    }
}

public static class MelodeeBlazorHeaderMiddlewareExtensions
{
    public static IApplicationBuilder UseMelodeeBlazorHeader(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<MelodeeBlazorCookieMiddleware>();
    }
}
