using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Models.OpenSubsonic.Requests;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Blazor.Controllers.OpenSubsonic;

public class PlaylistController(ISerializer serializer, EtagRepository etagRepository, OpenSubsonicApiService openSubsonicApiService, IMelodeeConfigurationFactory configurationFactory) : ControllerBase(etagRepository, serializer, configurationFactory)
{
    /// <summary>
    ///     Deletes a saved playlist.
    /// </summary>
    /// <param name="id">ID of the playlist to delete, as obtained by getPlaylists.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/deletePlaylist.view")]
    [Route("/rest/deletePlaylist")]
    public Task<IActionResult> DeletePlaylistAsync(string id, CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.DeletePlaylistAsync(id, ApiRequest, cancellationToken));
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
    [Route("/rest/createPlaylist")]
    public Task<IActionResult> CreatePlaylistAsync(string? id, string? name, string[]? songId, CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.CreatePlaylistAsync(id, name, songId, ApiRequest, cancellationToken));
    }


    /// <summary>
    ///     Updates a playlist. Only the owner of a playlist is allowed to update it..
    /// </summary>
    /// <param name="updateRequest">Model for request.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/updatePlaylist.view")]
    [Route("/rest/updatePlaylist")]
    public Task<IActionResult> CreatePlaylistAsync(UpdatePlayListRequest updateRequest, CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.UpdatePlaylistAsync(updateRequest, ApiRequest, cancellationToken));
    }

    /// <summary>
    ///     Returns all playlists a user is allowed to play.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getPlaylists.view")]
    [Route("/rest/getPlaylists")]
    public Task<IActionResult> GetPlaylistsAsync(CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.GetPlaylistsAsync(ApiRequest, cancellationToken));
    }

    /// <summary>
    ///     Returns a listing of files in a saved playlist.
    /// </summary>
    /// <param name="id">ID of the playlist to return, as obtained by getPlaylists.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getPlaylist.view")]
    [Route("/rest/getPlaylist")]
    public Task<IActionResult> GetPlaylistAsync(string id, CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.GetPlaylistAsync(id, ApiRequest, cancellationToken));
    }
}
