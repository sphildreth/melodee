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
    ///     Returns all configured top-level music folders.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("/rest/getMusicFolders.view")]
    public async Task<IActionResult> GetIndexes(CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.GetMusicFolders(ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }

    /// <summary>
    ///     Returns an indexed structure of all artists.
    /// </summary>
    /// <param name="musicFolderId">If specified, only return artists in the music folder with the given ID. See getMusicFolders.</param> 
    /// <param name="ifModifiedSince">If specified, only return a result if the artist collection has changed since the given time (in milliseconds since 1 Jan 1970).</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("/rest/getIndexes.view")]
    public async Task<IActionResult> GetIndexes(Guid? musicFolderId, long? ifModifiedSince, CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.GetIndexesAsync(musicFolderId, ifModifiedSince, ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }     

    /// <summary>
    ///     Returns a listing of all files in a music directory. Typically used to get list of albums for an artist, or list of songs for an album.
    /// </summary>
    /// <param name="id">A string which uniquely identifies the music folder. Obtained by calls to getIndexes or getMusicDirectory.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("/rest/getMusicDirectory.view")]
    public async Task<IActionResult> GetMusicDirectory(string id, CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.GetMusicDirectoryAsync(id, ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }    

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
