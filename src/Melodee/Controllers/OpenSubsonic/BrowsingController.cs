using Melodee.Common.Models.OpenSubsonic.Requests;
using Melodee.Common.Models.OpenSubsonic.Responses;
using Melodee.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Controllers.OpenSubsonic;

public class BrowsingController(ApiService apiService) : ControllerBase
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
    {
        
        return new JsonResult(new GetAlbumList2Response
        {
            ResponseData = new ApiResponse(
                "ok",
                "1.16.1",
                "Melodee",
                "0.1.1 (tag)",
                true,
                null
            ),
            AlbumList2 = []
        });
    }
}
