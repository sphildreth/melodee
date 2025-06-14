using System.Security.Claims;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Services;
using Melodee.Common.Utility;
using ServiceStack.Auth;

namespace Melodee.Common.Models;

public record UserInfo(int Id, Guid ApiKey, string UserName, string Email, string PublicKey, string PasswordEncrypted)
{
    public List<string>? Roles { get; init; }

    public static UserInfo BlankUserInfo => new(0, Guid.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

    public ClaimsPrincipal ToClaimsPrincipal(IMelodeeConfiguration configuration, string userAvatarPath)
    {
        var userSalt = UserService.GenerateSalt();
        var usersPassword = this.Decrypt(PasswordEncrypted, configuration);
        var userToken = $"{usersPassword}{userSalt}".ToMd5Hash();

        return new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new(ClaimTypes.PrimarySid, Id.ToString()),
                new(ClaimTypes.Sid, ApiKey.ToString()),
                new(ClaimTypes.Name, UserName),
                new(ClaimTypes.Email, Email),
                new(ClaimTypeRegistry.UserSalt, userSalt),
                new(ClaimTypeRegistry.UserPublicKey, PublicKey),
                new(ClaimTypeRegistry.UserToken, userToken),
                new(ClaimTypeRegistry.PasswordEncrypted, PasswordEncrypted)
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
            principal.FindFirst(ClaimTypes.Email)?.Value ?? "",
            principal.FindFirst(ClaimTypeRegistry.UserPublicKey)?.Value ?? "",
            principal.FindFirst(ClaimTypeRegistry.PasswordEncrypted)?.Value ?? ""
        )
        {
            Roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
        };
    }
}
