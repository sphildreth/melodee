using System.Diagnostics;
using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Models;
using Melodee.Common.Serialization;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Melodee.Common.Models.OpenSubsonic.Requests;
using Melodee.Common.Models.Scrobbling;
using Microsoft.IdentityModel.Tokens;


namespace Melodee.Blazor.Controllers.Melodee;

public abstract class ControllerBase(
    EtagRepository etagRepository,
    ISerializer serializer,
    IConfiguration configuration,
    IMelodeeConfigurationFactory configurationFactory) : CommonBase
{
    public EtagRepository EtagRepository { get; } = etagRepository;
    public ISerializer Serializer { get; } = serializer;
    protected IConfiguration Configuration { get; } = configuration;
    public IMelodeeConfigurationFactory ConfigurationFactory { get; } = configurationFactory;

    /// <summary>
    /// Validates a JWT token, ensures it's not expired, and returns the claims if valid.
    /// </summary>
    /// <param name="token">The JWT token to validate</param>
    /// <returns>A result object containing validation status and claims if valid</returns>
    protected TokenValidationResult ValidateToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return new TokenValidationResult
            {
                IsValid = false,
                ErrorMessage = "Token is null or empty"
            };
        }

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            // Use either MelodeeAuthSettings:Token or Jwt:Key based on configuration
            string? secretKey = Configuration.GetSection("MelodeeAuthSettings:Token").Value;
            if (string.IsNullOrEmpty(secretKey))
            {
                secretKey = Configuration.GetSection("Jwt:Key").Value;
            }

            if (string.IsNullOrEmpty(secretKey))
            {
                return new TokenValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "JWT secret key is not configured"
                };
            }

            var key = Encoding.UTF8.GetBytes(secretKey);

            // Token validation parameters
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false, // Set to true and configure if issuer validation is needed
                ValidateAudience = false, // Set to true and configure if audience validation is needed
                ValidateLifetime = true, // Validate token expiration
                ClockSkew = TimeSpan.Zero // No tolerance for token expiration time
            };

            // Validate the token and get the principal (contains claims)
            ClaimsPrincipal principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            // Additional check to ensure the token is not expired
            if (validatedToken.ValidTo < DateTime.UtcNow)
            {
                return new TokenValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Token has expired"
                };
            }

            return new TokenValidationResult
            {
                IsValid = true,
                ClaimsPrincipal = principal,
                Claims = principal.Claims.ToList(),
                Expiration = validatedToken.ValidTo
            };
        }
        catch (SecurityTokenExpiredException)
        {
            return new TokenValidationResult
            {
                IsValid = false,
                ErrorMessage = "Token has expired"
            };
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            return new TokenValidationResult
            {
                IsValid = false,
                ErrorMessage = "Invalid token signature"
            };
        }
        catch (Exception ex)
        {
            return new TokenValidationResult
            {
                IsValid = false,
                ErrorMessage = $"Token validation failed: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Class to hold token validation results
    /// </summary>
    protected record TokenValidationResult
    {
        /// <summary>
        /// Indicates whether the token is valid
        /// </summary>
        public bool IsValid { get; init; }

        /// <summary>
        /// Error message if validation fails
        /// </summary>
        public string? ErrorMessage { get; init; }

        /// <summary>
        /// The ClaimsPrincipal extracted from the token if valid
        /// </summary>
        public ClaimsPrincipal? ClaimsPrincipal { get; init; }

        /// <summary>
        /// List of claims from the token if valid
        /// </summary>
        public List<Claim>? Claims { get; init; }

        /// <summary>
        /// Token expiration date
        /// </summary>
        public DateTime Expiration { get; init; }

        /// <summary>
        /// Gets a specific claim value by type
        /// </summary>
        /// <param name="claimType">The type of claim to retrieve</param>
        /// <returns>The claim value or null if not found</returns>
        public string? GetClaimValue(string claimType)
        {
            return Claims?.FirstOrDefault(c => c.Type == claimType)?.Value;
        }
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var values = new List<KeyValue>();
        foreach (var header in context.HttpContext.Request.Headers)
        {
            values.Add(new KeyValue(header.Key, header.Value.ToString()));
        }

        var token = values.FirstOrDefault(x => x.Key == "Authorization")?.Value?.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase) ?? string.Empty;
        var tokenValidationResult = ValidateToken(token);
        if (tokenValidationResult.IsValid)
        {
            values.Add(new KeyValue("QueryString", context.HttpContext.Request.QueryString.ToString()));
            ApiRequest = new ApiRequest
            (
                values.ToArray(),
                true,
                tokenValidationResult.GetClaimValue(ClaimTypes.Name),
                null,
                null,
                tokenValidationResult.GetClaimValue(ClaimTypes.Sid),
                null,
                null,
                null,
                null,
                null,
                new UserPlayer
                (
                    values.FirstOrDefault(x => x.Key == "User-Agent")?.Value,
                    values.FirstOrDefault(x => x.Key == "c")?.Value,
                    values.FirstOrDefault(x => x.Key == "Host")?.Value,
                    GetRequestIp(context.HttpContext)
                )
            );
            Trace.WriteLine($"-*-> User [{ApiRequest.Username}] : {Serializer.Serialize(ApiRequest)}");
        }
        await next().ConfigureAwait(false);
    }
}
