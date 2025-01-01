using Melodee.Blazor.Filters;
using Melodee.Common.Serialization;
using Melodee.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Blazor.Controllers.OpenSubsonic;

public class BookmarksController(ISerializer serializer, EtagRepository etagRepository, OpenSubsonicApiService openSubsonicApiService) : ControllerBase(etagRepository, serializer)
{
    /// <summary>
    ///     Deletes a bookmark.
    /// </summary>
    /// <param name="id">ID of the media file for which to delete the bookmark. Other users’ bookmarks are not affected.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/deleteBookmark.view")]
    [Route("/rest/deleteBookmark")]
    public Task<IActionResult> DeleteBookmarkAsync(string id, CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.DeleteBookmarkAsync(id, ApiRequest, cancellationToken));
    }

    /// <summary>
    ///     Creates or updates a bookmark.
    /// </summary>
    /// <param name="id">ID of the media file to bookmark. If a bookmark already exists for this file it will be overwritten.</param>
    /// <param name="position">The position (in milliseconds) within the media file.</param>
    /// <param name="comment">A user-defined comment.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/createBookmark.view")]
    [Route("/rest/createBookmark")]
    public Task<IActionResult> CreateBookmarkAsync(string id, int position, string? comment, CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.CreateBookmarkAsync(id, position, comment, ApiRequest, cancellationToken));
    }

    /// <summary>
    ///     Returns all bookmarks for this user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getBookmarks.view")]
    [Route("/rest/getBookmarks")]
    public Task<IActionResult> GetBookmarksAsync(CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.GetBookmarksAsync(ApiRequest, cancellationToken));
    }

    /// <summary>
    ///     Returns the state of the play queue for this user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getPlayQueue.view")]
    [Route("/rest/getPlayQueue")]
    public Task<IActionResult> GetPlayQueueAsync(CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.GetPlayQueueAsync(ApiRequest, cancellationToken));
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
    [Route("/rest/savePlayQueue")]
    public Task<IActionResult> SavePlayQueueAsync(string[]? id, string? current, double? position, CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.SavePlayQueueAsync(id, current, position, ApiRequest, cancellationToken));
    }
}
