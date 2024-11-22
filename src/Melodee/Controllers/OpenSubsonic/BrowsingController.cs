using Melodee.Common.Serialization;
using Melodee.Results;
using Melodee.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Controllers.OpenSubsonic;

public class BrowsingController(ISerializer serializer, OpenSubsonicApiService openSubsonicApiService) : ControllerBase
{
    //getAlbumInfo
    //getAlbumInfo2
    //getArtist
    //getArtistInfo
    //getArtistInfo2
    //getArtists
    //getIndexes // To browse using file structure 
    //getMusicDirectory // To browse using file structure 
    //getMusicFolders
    //getSimilarSongs
    //getSimilarSongs2
    //getSong
    //getTopSongs
    //getVideoInfo
    //getVideos

    /// <summary>
    ///     Returns details for a song.
    /// </summary>
    /// <param name="id">The song id.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("/rest/getSong.view")]
    public async Task<IActionResult> GetSong(Guid id, CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.GetSongAsync(id, ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }    

    /// <summary>
    ///     Returns all genres.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("/rest/getGenres.view")]
    public async Task<IActionResult> GetGenres(CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.GetGenresAsync(ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }

    /// <summary>
    ///     Returns details for an album, including a list of songs. This method organizes music according to ID3 tags..
    /// </summary>
    /// <param name="id">ApiKey for the Album</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("/rest/getAlbum.view")]
    public async Task<IActionResult> GetAlbum(Guid id, CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.GetAlbumAsync(id, ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }
}
