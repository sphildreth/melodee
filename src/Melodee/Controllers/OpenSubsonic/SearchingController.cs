using Melodee.Common.Models.OpenSubsonic.Requests;
using Melodee.Common.Serialization;
using Melodee.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Controllers.OpenSubsonic;

public class SearchingController(ISerializer serializer, OpenSubsonicApiService openSubsonicApiService) : ControllerBase(serializer)
{
    // Deprecated says to use search2
    //search

    // By file structure, versus ID tags
    //search2


    /// <summary>
    ///     Returns albums, artists and songs matching the given search criteria. Supports paging through the result.
    /// </summary>
    /// <param name="request">Search request options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/search3.view")]
    public Task<IActionResult> SearchAsync(SearchRequest request, CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.SearchAsync(request, ApiRequest, cancellationToken));
    }
}
