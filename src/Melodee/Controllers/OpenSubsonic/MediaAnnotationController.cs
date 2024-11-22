using Melodee.Common.Serialization;
using Melodee.Results;
using Melodee.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Controllers.OpenSubsonic;

public class MediaAnnotationController(ISerializer serializer, OpenSubsonicApiService openSubsonicApiService) : ControllerBase
{
    //star
    //unstar
    //setRating
    //scrobble

    /// <summary>
    ///     Registers the local playback of one or more media files.
    /// </summary>
    /// <param name="id">A string which uniquely identifies the file to scrobble.</param>
    /// <param name="time">The time (in milliseconds since 1 Jan 1970) at which the song was listened to.</param>
    /// <param name="submission">Whether this is a “submission” or a “now playing” notification.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("/rest/scrobble.view")]
    public async Task<IActionResult> Scrobble(string id, double? time, bool? submission, CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.ScrobbleAsync(id, time, submission, ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }
}
