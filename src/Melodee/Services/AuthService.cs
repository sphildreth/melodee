using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Melodee.Services;

/// <summary>
///     Store and manage the current user's authentication state as a browser Session JWT and in Server Side Blazor
/// </summary>
public class AuthService(ICustomSessionService sessionService, IConfiguration configuration)
    : IAuthService
{
    private const string AuthTokenName = "melodee_auth_token";
    private ClaimsPrincipal? _currentUser;

    public event Action<ClaimsPrincipal>? UserChanged;

    public ClaimsPrincipal CurrentUser
    {
        get => _currentUser ?? new ClaimsPrincipal();
        set
        {
            _currentUser = value;

            if (UserChanged is not null)
            {
                UserChanged(_currentUser);
            }
        }
    }

    public bool IsLoggedIn => CurrentUser.Identity?.IsAuthenticated ?? false;

    public async Task LogoutAsync()
    {
        //Update the Blazor Server State for the user to an anonymous user
        CurrentUser = new ClaimsPrincipal();

        //Remove the JWT from the browser session
        var authToken = await sessionService.GetItemAsStringAsync(AuthTokenName);

        if (!string.IsNullOrEmpty(authToken))
        {
            await sessionService.RemoveItemAsync(AuthTokenName);
        }
    }


    /// <summary>
    ///     If the user somehow loses their server session, this method will attempt to restore the state from the JWT in the
    ///     browser session
    /// </summary>
    /// <returns>True if the state was restored</returns>
    public async Task<bool> GetStateFromTokenAsync()
    {
        var result = false;
        var authToken = await sessionService.GetItemAsStringAsync(AuthTokenName);

        var identity = new ClaimsIdentity();

        if (!string.IsNullOrEmpty(authToken))
        {
            try
            {
                //Ensure the JWT is valid
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(configuration.GetSection("MelodeeAuthSettings:Token").Value!);

                tokenHandler.ValidateToken(authToken, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out var validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
                result = true;
            }
            catch
            {
                //If the JWT is invalid, remove it from the session
                await sessionService.RemoveItemAsync(AuthTokenName);

                //This is an anonymous user
                identity = new ClaimsIdentity();
            }
        }

        var user = new ClaimsPrincipal(identity);

        //Update the Blazor Server State for the user
        CurrentUser = user;
        return result;
    }


    public async Task Login(ClaimsPrincipal user)
    {
        CurrentUser = user;
        var tokenEncryptionKey = configuration.GetSection("MelodeeAuthSettings:Token").Value!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenEncryptionKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        var tokenHoursString = configuration.GetSection("MelodeeAuthSettings:TokenHours").Value;
        int.TryParse(tokenHoursString, out var tokenHours);
        var token = new JwtSecurityToken(
            claims: user.Claims,
            expires: DateTime.Now.AddHours(tokenHours),
            signingCredentials: creds);
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        await sessionService.SetItemAsStringAsync(AuthTokenName, jwt);
    }
}
