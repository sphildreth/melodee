using Ardalis.GuardClauses;
using Melodee.Common.Models.OpenSubsonic.Requests;
using Melodee.Common.Models.OpenSubsonic.Responses;
using Melodee.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Controllers.OpenSubsonic;

public class BrowsingController(OpenSubsonicApiService openSubsonicApiService) : ControllerBase
{
    
    //getAlbum
    //getAlbumInfo
    //getAlbumInfo2
    //getArtist
    //getArtistInfo
    //getArtistInfo2
    //getArtists
    //getGenres
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
    /// Returns all genres.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("/rest/getGenres.view")]
    public async Task<IActionResult> GetGenres(CancellationToken cancellationToken = default)
        => new JsonResult(await openSubsonicApiService.GetGenresAsync(ApiRequest, cancellationToken).ConfigureAwait(false));
}
