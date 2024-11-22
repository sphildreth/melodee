using Melodee.Common.Models.OpenSubsonic;
using Melodee.Common.Models.OpenSubsonic.Requests;
using Melodee.Common.Models.OpenSubsonic.Responses;
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
    // getPlayQueue
    // savePlayQueue

    /// <summary>
    /// Saves the state of the play queue for this user.
    /// </summary>
    /// <param name="apiKeys">Array of ApiKsy of a song in the play queue. Use one id parameter for each song in the play queue. Note id is optional. Send a call without any parameters to clear the currently saved queue.</param>
    /// <param name="position">The position in milliseconds within the currently playing song.</param>
    /// <param name="current">The ID of the current playing song.</param> 
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("/rest/savePlayQueue.view")]
    public async Task<IActionResult> SavePlayQueue(Guid[]? apiKeys, Guid? current, double? position, CancellationToken cancellationToken = default)
        => new JsonStringResult(serializer.Serialize(await openSubsonicApiService.SavePlayQueue(apiKeys, current, position, ApiRequest, cancellationToken).ConfigureAwait(false))!);
    
}
