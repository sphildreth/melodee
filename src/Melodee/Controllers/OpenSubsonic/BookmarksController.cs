using Melodee.Common.Serialization;
using Melodee.Results;
using Melodee.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Controllers.OpenSubsonic;

public class BookmarksController(ISerializer serializer, OpenSubsonicApiService openSubsonicApiService) : ControllerBase
{
    // getBookmarks
    // createBookmark
    // deleteBookmark

    /// <summary>
    ///     Returns the state of the play queue for this user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getPlayQueue.view")]
    public async Task<IActionResult> GetPlayQueueAsync(CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.GetPlayQueueAsync(ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }

    /// <summary>
    ///     Saves the state of the play queue for this user.
    /// </summary>
    /// <param name="id">
    ///     Array of ApiKeys of a song in the play queue. Use one id parameter for each song in the play queue.
    ///     Note id is optional. Send a call without any parameters to clear the currently saved queue.
    /// </param>
    /// <param name="position">The position in milliseconds within the currently playing song.</param>
    /// <param name="current">The ID of the current playing song.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/savePlayQueue.view")]
    public async Task<IActionResult> SavePlayQueueAsync(string[]? id, string? current, double? position, CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.SavePlayQueueAsync(id, current, position, ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }
}
