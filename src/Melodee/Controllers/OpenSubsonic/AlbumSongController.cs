using Melodee.Common.Models.OpenSubsonic.Requests;
using Melodee.Common.Serialization;
using Melodee.Results;
using Melodee.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Controllers.OpenSubsonic;

public class AlbumSongController(ISerializer serializer, OpenSubsonicApiService openSubsonicApiService) : ControllerBase
{
    // These are by file structure, versus ID tags
    //getAlbumList


    /// <summary>
    ///     Returns random songs matching the given criteria.
    /// </summary>
    /// <param name="size">The maximum number of songs to return. Max 500.</param>
    /// <param name="genre">Only returns songs belonging to this genre.</param>
    /// <param name="toYear">Only return songs published after or in this year.</param>
    /// <param name="fromYear">Only return songs published before or in this year.</param>    
    /// <param name="musicFolderId">Only return results from the music folder with the given ID. See getMusicFolders.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getRandomSongs.view")]
    public async Task<IActionResult> GetRandomSongsAsync(int size, string? genre, int? fromYear, int? toYear, string? musicFolderId, CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.GetRandomSongsAsync(size, genre, fromYear, toYear, musicFolderId, ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }     

    /// <summary>
    ///     Returns songs in a given genre.
    /// </summary>
    /// <param name="genre">The genre, as returned by getGenres.</param>
    /// <param name="count">The maximum number of songs to return. Max 500.</param>     
    /// <param name="offset">The offset. Useful if you want to page through the songs in a genre.</param>
    /// <param name="musicFolderId">Only return results from the music folder with the given ID. See getMusicFolders.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getSongsByGenre.view")]
    public async Task<IActionResult> GetSongsByGenre(string genre, int? count, int? offset, string? musicFolderId, CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.GetSongsByGenreAsync(genre, count, offset, musicFolderId, ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }     

    /// <summary>
    ///     Returns starred songs, albums and artists.
    /// </summary>
    /// <param name="musicFolderId">Only return results from the music folder with the given ID. See getMusicFolders.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getStarred.view")]
    public async Task<IActionResult> GetStarredAsync(string? musicFolderId, CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.GetStarredAsync("starred", musicFolderId, ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }    
    
    /// <summary>
    ///     Returns starred songs, albums and artists. Similar to getStarred, but organizes music according to ID3 tags.
    /// </summary>
    /// <param name="musicFolderId">Only return results from the music folder with the given ID. See getMusicFolders.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getStarred2.view")]
    public async Task<IActionResult> GetStarred2Async(string? musicFolderId, CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.GetStarredAsync("starred2", musicFolderId, ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }     
    
    /// <summary>
    ///     Returns what is currently being played by all users.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getNowPlaying.view")]
    public async Task<IActionResult> GetNowPlayingAsync(CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.GetNowPlayingAsync(ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }

    /// <summary>
    ///     Returns a list of random, newest, highest rated etc. albums.
    /// </summary>
    /// <param name="getAlbumListRequest">Request model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getAlbumList2.view")]
    public async Task<IActionResult> GetAlbumList2Async(GetAlbumListRequest getAlbumListRequest, CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.GetAlbumList2Async(getAlbumListRequest, ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }
}
