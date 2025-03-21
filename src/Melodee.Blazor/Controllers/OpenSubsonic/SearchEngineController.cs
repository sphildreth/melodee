using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Serialization;
using Melodee.Common.Services.SearchEngines;
using Melodee.Results;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Blazor.Controllers.OpenSubsonic;

/// <summary>
///     This is a Melodee extension to the OpenSubsonic API and is not standard OpenSubsonic search, for that
///     <see cref="SearchingController" />
/// </summary>
public class SearchEngineController(
    ISerializer serializer,
    EtagRepository etagRepository,
    ArtistSearchEngineService artistSearchEngineService,
    ArtistImageSearchEngineService artistImageSearchEngineService,
    AlbumImageSearchEngineService albumImageSearchEngineService,
    IMelodeeConfigurationFactory configurationFactory) : ControllerBase(etagRepository, serializer, configurationFactory)
{
    /// <summary>
    ///     Perform an artist search engine search and return results.
    /// </summary>
    /// <param name="query">Query request for search</param>
    /// <param name="maxResult">Maximum number of results to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/searchForArtist.view")]
    [Route("/rest/searchForArtist")]
    public async Task<IActionResult> SearchForArtistAsync(ArtistQuery query, int? maxResult, CancellationToken cancellationToken = default)
    {
        await artistSearchEngineService.InitializeAsync(null, cancellationToken);
        return new JsonStringResult(Serializer.Serialize(await artistSearchEngineService.DoSearchAsync(query, maxResult, cancellationToken).ConfigureAwait(false))!);
    }

    /// <summary>
    ///     Perform an album image search engine search and return results.
    /// </summary>
    /// <param name="query">Query request for search</param>
    /// <param name="maxResult">Maximum number of results to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/searchForAlbumImage.view")]
    [Route("/rest/searchForAlbumImage")]
    public async Task<IActionResult> SearchForAlbumImageAsync(AlbumQuery query, int? maxResult, CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(Serializer.Serialize(await albumImageSearchEngineService.DoSearchAsync(query, maxResult, cancellationToken).ConfigureAwait(false))!);
    }

    /// <summary>
    ///     Perform an artist image search engine search and return results.
    /// </summary>
    /// <param name="query">Query request for search</param>
    /// <param name="maxResult">Maximum number of results to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/searchForArtistImage.view")]
    [Route("/rest/searchForArtistImage")]
    public async Task<IActionResult> SearchForAlbumImageAsync(ArtistQuery query, int? maxResult, CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(Serializer.Serialize(await artistImageSearchEngineService.DoSearchAsync(query, maxResult, cancellationToken).ConfigureAwait(false))!);
    }
}
