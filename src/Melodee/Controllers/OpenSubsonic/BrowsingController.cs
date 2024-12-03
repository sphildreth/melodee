using Melodee.Common.Serialization;
using Melodee.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Controllers.OpenSubsonic;

public class BrowsingController(ISerializer serializer, OpenSubsonicApiService openSubsonicApiService) : ControllerBase(serializer)
{
    //getArtists
    //getSimilarSongs
    //getSimilarSongs2
    //getTopSongs
    //getVideoInfo
    //getVideos

    /// <summary>
    ///     Returns album info.
    /// </summary>
    /// <param name="id">The artist, album or song ID.</param>
    /// <param name="count">Max number of similar artists to return.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getAlbumInfo.view")]
    [Route("/rest/getAlbumInfo2.view")]
    public Task<IActionResult> GetAlbumInfo(string id, CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.GetAlbumInfoAsync(id, ApiRequest, cancellationToken));
    }

    /// <summary>
    ///     Returns artist info.
    /// </summary>
    /// <param name="id">The artist, album or song ID.</param>
    /// <param name="count">Max number of similar artists to return.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getArtistInfo.view")]
    [Route("/rest/getArtistInfo2.view")]
    public Task<IActionResult> GetArtistInfo(string id, int? count, CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.GetArtistInfoAsync(id, count, ApiRequest, cancellationToken));
    }

    /// <summary>
    ///     Returns top songs for the given artist.
    /// </summary>
    /// <param name="artist">The artist name.</param>
    /// <param name="count">Max number of songs to return.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getTopSongs.view")]
    public Task<IActionResult> GetTopSongs(string artist, int? count, CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.GetTopSongsAsync(artist, count, ApiRequest, cancellationToken));
    }

    /// <summary>
    ///     Returns details for an artist.
    /// </summary>
    /// <param name="id">The artist ID.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getArtist.view")]
    public Task<IActionResult> GetArtistAsync(string id, CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.GetArtistAsync(id, ApiRequest, cancellationToken));
    }

    /// <summary>
    ///     Returns all configured top-level music folders.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getMusicFolders.view")]
    public Task<IActionResult> GetMusicFolders(CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.GetMusicFolders(ApiRequest, cancellationToken));
    }

    /// <summary>
    ///     Returns an indexed structure of all artists.
    /// </summary>
    /// <param name="musicFolderId">
    ///     If specified, only return artists in the music folder with the given ID. See
    ///     getMusicFolders.
    /// </param>
    /// <param name="ifModifiedSince">
    ///     If specified, only return a result if the artist collection has changed since the given
    ///     time (in milliseconds since 1 Jan 1970).
    /// </param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getIndexes.view")]
    public Task<IActionResult> GetIndexesAsync(Guid? musicFolderId, long? ifModifiedSince, CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.GetIndexesAsync("indexes", musicFolderId, ifModifiedSince, ApiRequest, cancellationToken));
    }

    /// <summary>
    ///     Returns all artists.
    /// </summary>
    /// <param name="musicFolderId">
    ///     If specified, only return artists in the music folder with the given ID. See
    ///     getMusicFolders.
    /// </param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getArtists.view")]
    public Task<IActionResult> GetArtistsAsync(Guid? musicFolderId, CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.GetIndexesAsync("artists", musicFolderId, null, ApiRequest, cancellationToken));
    }

    /// <summary>
    ///     Returns a listing of all files in a music directory. Typically used to get list of albums for an artist, or list of
    ///     songs for an album.
    /// </summary>
    /// <param name="id">
    ///     A string which uniquely identifies the music folder. Obtained by calls to getIndexes or
    ///     getMusicDirectory.
    /// </param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getMusicDirectory.view")]
    public Task<IActionResult> GetMusicDirectoryAsync(string id, CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.GetMusicDirectoryAsync(id, ApiRequest, cancellationToken));
    }

    /// <summary>
    ///     Returns details for a song.
    /// </summary>
    /// <param name="id">The song id.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getSong.view")]
    public Task<IActionResult> GetSongAsync(string id, CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.GetSongAsync(id, ApiRequest, cancellationToken));
    }

    /// <summary>
    ///     Returns all genres.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getGenres.view")]
    public Task<IActionResult> GetGenresAsync(CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.GetGenresAsync(ApiRequest, cancellationToken));
    }

    /// <summary>
    ///     Returns details for an album, including a list of songs. This method organizes music according to ID3 tags..
    /// </summary>
    /// <param name="id">ApiKey for the Album</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getAlbum.view")]
    public Task<IActionResult> GetAlbumAsync(string id, CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.GetAlbumAsync(id, ApiRequest, cancellationToken));
    }
}
