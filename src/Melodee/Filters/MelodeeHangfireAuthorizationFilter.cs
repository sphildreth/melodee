using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Hangfire.Dashboard;
using Melodee.Common.Constants;
using Microsoft.IdentityModel.Tokens;

namespace Melodee.Filters;

public class MelodeeHangfireAuthorizationFilter(string melodeeAuthSettingsToken) : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        var jwtCookie = httpContext.Request.Cookies["jwt"];
        if (jwtCookie != null)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(melodeeAuthSettingsToken);
            tokenHandler.ValidateToken(jwtCookie, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);
            var jwtToken = (JwtSecurityToken)validatedToken;
            return jwtToken?.Claims?.Any(x => x.Type == ClaimTypes.Role && x.Value == RoleNameRegistry.Administrator) ?? false;
        }
        return false;

    }
}
