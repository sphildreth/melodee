using Melodee.Common.Serialization;
using Melodee.Results;
using Melodee.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Controllers.OpenSubsonic;

public class MediaAnnotationController(ISerializer serializer, OpenSubsonicApiService openSubsonicApiService) : ControllerBase
{
    /// <summary>
    ///     Sets the rating for a music file.
    /// </summary>
    /// <param name="id">A string which uniquely identifies the file (song) or folder (album/artist) to ra</param>
    /// <param name="rating">The rating between 1 and 5 (inclusive), or 0 to remove the rating.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/setRating.view")]
    public async Task<IActionResult> SetRatingAsync(string id, int rating, CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.SetRatingAsync(id, rating, ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }    

    /// <summary>
    ///     Registers the local playback of one or more media files.
    /// </summary>
    /// <param name="id">Array of Song ApiKeyIds to scrobble.</param>
    /// <param name="time">The times (in milliseconds since 1 Jan 1970) at which the song was listened to.</param>
    /// <param name="submission">Whether this is a “submission” or a “now playing” notification.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/scrobble.view")]
    public async Task<IActionResult> ScrobbleAsync(string[] id, double[]? time, bool? submission, CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.ScrobbleAsync(id, time, submission, ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }
    
    /// <summary>
    ///     Attaches a star to a song, album or artist.
    /// </summary>
    /// <param name="id">The ID of the file (song) or folder (album/artist) to star. Multiple parameters allowed.</param>
    /// <param name="albumId">The ID of an album to star. Use this rather than id if the client accesses the media collection according to ID3 tags rather than file structure. Multiple parameters allowed.</param>
    /// <param name="artistId">The ID of an artist to star. Use this rather than id if the client accesses the media collection according to ID3 tags rather than file structure. Multiple parameters allowed.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/star.view")]
    public async Task<IActionResult> StarAsync(string id, string? albumId, string? artistId, CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.ToggleStarAsync(true, id, albumId, artistId, ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }    
    
    /// <summary>
    ///     Removes a star to a song, album or artist.
    /// </summary>
    /// <param name="id">The ID of the file (song) or folder (album/artist) to star. Multiple parameters allowed.</param>
    /// <param name="albumId">The ID of an album to star. Use this rather than id if the client accesses the media collection according to ID3 tags rather than file structure. Multiple parameters allowed.</param>
    /// <param name="artistId">The ID of an artist to star. Use this rather than id if the client accesses the media collection according to ID3 tags rather than file structure. Multiple parameters allowed.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/unstar.view")]
    public async Task<IActionResult> UnstarAsync(string id, string? albumId, string? artistId, CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.ToggleStarAsync(false, id, albumId, artistId, ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }     
}
