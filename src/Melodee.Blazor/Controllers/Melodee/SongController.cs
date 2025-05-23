using Asp.Versioning;
using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Blazor.Controllers.Melodee;

[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class SongController(
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
    [HttpGet]
    [HttpGet]
    [Route("stream")]
    public Task<IActionResult> StreamSong(Guid apiKey, CancellationToken cancellationToken = default)
    {
        // var streamResult = await openSubsonicApiService.StreamAsync(request, ApiRequest, cancellationToken).ConfigureAwait(false);
        // if (streamResult.IsSuccess)
        // {
        //     foreach (var responseHeader in streamResult.ResponseHeaders)
        //     {
        //         Response.Headers[responseHeader.Key] = responseHeader.Value;
        //     }
        //
        //     await Response.Body
        //         .WriteAsync(streamResult.Bytes.AsMemory(0, streamResult.Bytes.Length), cancellationToken)
        //         .ConfigureAwait(false);
        //     return new EmptyResult();
        // }
        //
        // Response.StatusCode = (int)HttpStatusCode.NotFound;
        // return new JsonStringResult(Serializer.Serialize(new ResponseModel
        // {
        //     UserInfo = UserInfo.BlankUserInfo,
        //     IsSuccess = false,
        //     ResponseData = await openSubsonicApiService.NewApiResponse(
        //         false,
        //         string.Empty,
        //         string.Empty,
        //         Error.DataNotFoundError)
        // })!);
        throw new NotImplementedException();
    }
}
