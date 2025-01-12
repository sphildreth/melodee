using System.Security.Claims;

namespace Melodee.Blazor.Services;

public interface IAuthService
{
    ClaimsPrincipal CurrentUser { get; set; }
    bool IsLoggedIn { get; }
    bool IsAdmin { get; }
    event Action<ClaimsPrincipal> UserChanged;
    Task<bool> GetStateFromTokenAsync();
    Task LogoutAsync();
    Task Login(ClaimsPrincipal user, bool? doRememberMe = null);
}
