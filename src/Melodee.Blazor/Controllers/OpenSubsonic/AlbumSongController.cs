using Melodee.Blazor.Filters;
using Melodee.Common.Models.OpenSubsonic.Requests;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Blazor.Controllers.OpenSubsonic;

public class AlbumSongController(ISerializer serializer, EtagRepository etagRepository, OpenSubsonicApiService openSubsonicApiService) : ControllerBase(etagRepository, serializer)
{
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
    [Route("/rest/getRandomSongs")]
    public Task<IActionResult> GetRandomSongsAsync(int size, string? genre, int? fromYear, int? toYear, string? musicFolderId, CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.GetRandomSongsAsync(size, genre, fromYear, toYear, musicFolderId, ApiRequest, cancellationToken));
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
    [Route("/rest/getSongsByGenre")]
    public Task<IActionResult> GetSongsByGenre(string genre, int? count, int? offset, string? musicFolderId, CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.GetSongsByGenreAsync(genre, count, offset, musicFolderId, ApiRequest, cancellationToken));
    }

    /// <summary>
    ///     Returns starred songs, albums and artists.
    /// </summary>
    /// <param name="musicFolderId">Only return results from the music folder with the given ID. See getMusicFolders.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getStarred.view")]
    [Route("/rest/getStarred")]
    public Task<IActionResult> GetStarredAsync(string? musicFolderId, CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.GetStarredAsync(musicFolderId, ApiRequest, cancellationToken));
    }

    /// <summary>
    ///     Returns starred songs, albums and artists. Similar to getStarred, but organizes music according to ID3 tags.
    /// </summary>
    /// <param name="musicFolderId">Only return results from the music folder with the given ID. See getMusicFolders.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getStarred2.view")]
    [Route("/rest/getStarred2")]
    public Task<IActionResult> GetStarred2Async(string? musicFolderId, CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.GetStarred2Async(musicFolderId, ApiRequest, cancellationToken));
    }

    /// <summary>
    ///     Returns what is currently being played by all users.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getNowPlaying.view")]
    [Route("/rest/getNowPlaying")]
    public Task<IActionResult> GetNowPlayingAsync(CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.GetNowPlayingAsync(ApiRequest, cancellationToken));
    }

    /// <summary>
    ///     Returns a list of random, newest, highest rated etc. albums.
    /// </summary>
    /// <param name="getAlbumListRequest">Request model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getAlbumList2.view")]
    [Route("/rest/getAlbumList2")]
    public Task<IActionResult> GetAlbumList2Async(GetAlbumListRequest getAlbumListRequest, CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.GetAlbumList2Async(getAlbumListRequest, ApiRequest, cancellationToken));
    }

    /// <summary>
    ///     Returns a list of random, newest, highest rated etc. albums.
    /// </summary>
    /// <param name="getAlbumListRequest">Request model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getAlbumList.view")]
    [Route("/rest/getAlbumList")]
    public Task<IActionResult> GetAlbumListAsync(GetAlbumListRequest getAlbumListRequest, CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.GetAlbumListAsync(getAlbumListRequest, ApiRequest, cancellationToken));
    }
}
