using Melodee.Common.Models.OpenSubsonic.Requests;
using Melodee.Common.Serialization;
using Melodee.Results;
using Melodee.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Controllers.OpenSubsonic;

public class PlaylistController(ISerializer serializer, OpenSubsonicApiService openSubsonicApiService) : ControllerBase
{
    //deletePlaylist

    /// <summary>
    ///     Deletes a saved playlist.
    /// </summary>
    /// <param name="id">ID of the playlist to delete, as obtained by getPlaylists.</param>
    /// <param name="cancellationToken">Cancellation token</param>     
    [HttpGet]
    [HttpPost]
    [Route("/rest/deletePlaylist.view")]
    public async Task<IActionResult> DeletePlaylistAsync(string id, CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.DeletePlaylistAsync(id, ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }
    
    /// <summary>
    ///     Creates (or updates) a playlist.
    /// </summary>
    /// <param name="id">The playlist ID (required if updating)</param>
    /// <param name="name">The human-readable name of the playlist (required if creating)</param>
    /// <param name="songId">ID of a song in the playlist. Use one songId parameter for each song in the playlist.</param>
    /// <param name="cancellationToken">Cancellation token</param>     
    [HttpGet]
    [HttpPost]
    [Route("/rest/createPlaylist.view")]
    public async Task<IActionResult> CreatePlaylistAsync(string? id, string? name, string[]? songId, CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.CreatePlaylistAsync(id, name, songId, ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }

    /// <summary>
    ///     Updates a playlist. Only the owner of a playlist is allowed to update it..
    /// </summary>
    /// <param name="updateRequest">Model for request.</param>
    /// <param name="cancellationToken">Cancellation token</param>     
    [HttpGet]
    [HttpPost]
    [Route("/rest/updatePlaylist.view")]
    public async Task<IActionResult> CreatePlaylistAsync(UpdatePlayListRequest updateRequest, CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.UpdatePlaylistAsync(updateRequest, ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }     

    /// <summary>
    ///     Returns all playlists a user is allowed to play.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getPlaylists.view")]
    public async Task<IActionResult> GetPlaylistsAsync(CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.GetPlaylistsAsync(ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }

    /// <summary>
    ///     Returns a listing of files in a saved playlist.
    /// </summary>
    /// <param name="id">ID of the playlist to return, as obtained by getPlaylists.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getPlaylist.view")]
    public async Task<IActionResult> GetPlaylistAsync(string id, CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.GetPlaylistAsync(id, ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }    
}
