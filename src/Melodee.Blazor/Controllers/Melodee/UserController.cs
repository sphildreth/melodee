using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Asp.Versioning;
using Melodee.Blazor.Controllers.Melodee.Extensions;
using Melodee.Blazor.Controllers.Melodee.Models;
using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Models;
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
    PlaylistService playlistService,
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
        return Ok(new {token = tokenHandler.WriteToken(token) });
    }
    
    /// <summary>
    /// Return information about the current user making the request.
    /// </summary>
    [HttpGet]
    [Route("me")]
    public async Task<IActionResult> AboutMeAsync(CancellationToken cancellationToken = default)
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
        return Ok(new {data= userResult.Data.ToUserModel(GetBaseUrl(Configuration))});
    }
    
    /// <summary>
    /// Return the last three songs played by the user
    /// </summary>
    [HttpGet]
    [Route("last3played")]
    public async Task<IActionResult> Last3PlayedSongsForUserAsync(CancellationToken cancellationToken = default)
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
        var userLastPlayedResult = await userService.UserLastPlayedSongsAsync(userResult.Data.Id, 3, cancellationToken);
        return Ok(new {data= userLastPlayedResult.Data.Where(x => x?.Song != null).Select(x => x!.Song.ToSongDataInfo()).ToArray()});
    }  
    
    [HttpGet]
    [Route("playlists")]
    public async Task<IActionResult> UsersPlaylistsAsync(short? limit, CancellationToken cancellationToken = default)
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
        var playlists = await playlistService.ListAsync(userResult.Data.ToUserInfo(), new PagedRequest { PageSize = limit }, cancellationToken);
        var baseUrl = GetBaseUrl(Configuration);
        return Ok(new {data= playlists.Data.Select(x => x.ToPlaylistModel(baseUrl, userResult.Data.ToUserModel(baseUrl))).ToArray()});
    } 
}
