using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Serialization;
using Melodee.Results;
using Melodee.Services.SearchEngines;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Controllers.OpenSubsonic;

public class SearchEngineController(ISerializer serializer, ArtistSearchEngineService artistSearchEngineService) : ControllerBase
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
    public async Task<IActionResult> SearchForArtistAsync(ArtistQuery query, int? maxResult, CancellationToken cancellationToken = default)
    {
        await artistSearchEngineService.InitializeAsync(null, cancellationToken);
        return new JsonStringResult(serializer.Serialize(await artistSearchEngineService.DoSearchAsync(query, maxResult, cancellationToken).ConfigureAwait(false))!);
    }
}