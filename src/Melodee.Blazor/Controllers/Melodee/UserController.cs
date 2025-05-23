using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Asp.Versioning;
using Melodee.Blazor.Controllers.Melodee.Extensions;
using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Melodee.Blazor.Controllers.Melodee;

[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class UserController(
    ISerializer serializer,
    EtagRepository etagRepository,
    UserService userService,
    IConfiguration configuration,
    IMelodeeConfigurationFactory configurationFactory) : ControllerBase(
    etagRepository,
    serializer,
    configuration,
    configurationFactory)
{
    [HttpPost]
    [Route("authenticate")]
    public async Task<IActionResult> AuthenticateUserAsync([FromBody] LoginModel model, CancellationToken cancellationToken = default)
    {
        var authResult = await userService.LoginUserAsync(model.Email, model.Password, cancellationToken);
        if (!authResult.IsSuccess || authResult.Data == null)
        {
            return Unauthorized();
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(Configuration.GetSection("MelodeeAuthSettings:Token").Value!);
        var tokenHoursString = Configuration.GetSection("MelodeeAuthSettings:TokenHours").Value;
        var tokenHours = SafeParser.ToNumber<int>(tokenHoursString);
        var tokenDescriptor = new SecurityTokenDescriptor {
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.Email, authResult.Data.Email),
                new Claim(ClaimTypes.Name, authResult.Data.UserName),
                new Claim(ClaimTypes.Sid, authResult.Data.ApiKey.ToString())
            ]),
            Expires = DateTime.Now.AddHours(tokenHours),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return Ok(new {status = 200, token = tokenHandler.WriteToken(token) });
    }
    
    [HttpGet]
    [Route("me")]
    public async Task<IActionResult> AmountMeAsync(CancellationToken cancellationToken = default)
    {
        if (!ApiRequest.IsAuthorized)
        {
            return Unauthorized(new { error = "Authorization token is missing" });
        }
        var userResult = await userService.GetByApiKeyAsync(SafeParser.ToGuid(ApiRequest.ApiKey) ?? Guid.Empty, cancellationToken);
        if (!userResult.IsSuccess || userResult.Data == null)
        {
            return Unauthorized(new { error = "Authorization token is invalid" });
        }
        return Ok(userResult.Data.ToUserModel(GetBaseUrl(Configuration)));
    }
    
    
    [HttpGet]
    [Route("last3played")]
    public Task<IActionResult> Last3PlayedSongsForUserAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }  
}

public record LoginModel(string Email, string Password);
