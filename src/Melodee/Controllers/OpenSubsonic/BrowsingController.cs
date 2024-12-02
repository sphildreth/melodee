using Melodee.Common.Serialization;
using Melodee.Results;
using Melodee.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Controllers.OpenSubsonic;

public class BrowsingController(ISerializer serializer, OpenSubsonicApiService openSubsonicApiService) : ControllerBase
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
    public async Task<IActionResult> GetAlbumInfo(string id, CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.GetAlbumInfoAsync(id, ApiRequest, cancellationToken).ConfigureAwait(false))!);
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
    public async Task<IActionResult> GetArtistInfo(string id, int? count, CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.GetArtistInfoAsync(id, count, ApiRequest, cancellationToken).ConfigureAwait(false))!);
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
    public async Task<IActionResult> GetTopSongs(string artist, int? count, CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.GetTopSongsAsync(artist, count, ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }

    /// <summary>
    ///     Returns details for an artist.
    /// </summary>
    /// <param name="id">The artist ID.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getArtist.view")]
    public async Task<IActionResult> GetArtistAsync(string id, CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.GetArtistAsync(id, ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }

    /// <summary>
    ///     Returns all configured top-level music folders.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getMusicFolders.view")]
    public async Task<IActionResult> GetMusicFolders(CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.GetMusicFolders(ApiRequest, cancellationToken).ConfigureAwait(false))!);
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
    public async Task<IActionResult> GetIndexesAsync(Guid? musicFolderId, long? ifModifiedSince, CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.GetIndexesAsync("indexes", musicFolderId, ifModifiedSince, ApiRequest, cancellationToken).ConfigureAwait(false))!);
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
    public async Task<IActionResult> GetArtistsAsync(Guid? musicFolderId, CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.GetIndexesAsync("artists", musicFolderId, null, ApiRequest, cancellationToken).ConfigureAwait(false))!);
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
    public async Task<IActionResult> GetMusicDirectoryAsync(string id, CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.GetMusicDirectoryAsync(id, ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }

    /// <summary>
    ///     Returns details for a song.
    /// </summary>
    /// <param name="id">The song id.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getSong.view")]
    public async Task<IActionResult> GetSongAsync(string id, CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.GetSongAsync(id, ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }

    /// <summary>
    ///     Returns all genres.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getGenres.view")]
    public async Task<IActionResult> GetGenresAsync(CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.GetGenresAsync(ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }

    /// <summary>
    ///     Returns details for an album, including a list of songs. This method organizes music according to ID3 tags..
    /// </summary>
    /// <param name="id">ApiKey for the Album</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getAlbum.view")]
    public async Task<IActionResult> GetAlbumAsync(string id, CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.GetAlbumAsync(id, ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }
}
