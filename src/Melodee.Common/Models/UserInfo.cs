using System.Security.Claims;
using Melodee.Common.Utility;

namespace Melodee.Common.Models;

public record UserInfo(int Id, Guid ApiKey, string UserName, string Email)
{
    public List<string>? Roles { get; init; }

    public ClaimsPrincipal ToClaimsPrincipal()
    {
        return new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new(ClaimTypes.PrimarySid, Id.ToString()),
                new(ClaimTypes.Sid, ApiKey.ToString()),
                new(ClaimTypes.Name, UserName),
                new(ClaimTypes.Email, Email)
            }.Concat(Roles?.Select(r => new Claim(ClaimTypes.Role, r)).ToArray() ?? []),
            "Melodee"));
    }

    public static UserInfo FromClaimsPrincipal(ClaimsPrincipal principal)
    {
        return new UserInfo
        (
            SafeParser.ToNumber<int>(principal.FindFirst(ClaimTypes.PrimarySid)?.Value ?? ""),
            SafeParser.ToGuid(principal.FindFirst(ClaimTypes.Sid)?.Value) ?? Guid.Empty,
            principal.FindFirst(ClaimTypes.Name)?.Value ?? "",
            principal.FindFirst(ClaimTypes.Email)?.Value ?? ""
        )
        {
            Roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
        };
    }
}
