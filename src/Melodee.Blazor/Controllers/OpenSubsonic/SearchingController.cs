using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Models.OpenSubsonic.Requests;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Blazor.Controllers.OpenSubsonic;

public class SearchingController(ISerializer serializer, EtagRepository etagRepository, OpenSubsonicApiService openSubsonicApiService, IMelodeeConfigurationFactory configurationFactory) : ControllerBase(etagRepository, serializer, configurationFactory)
{
    // Deprecated says to use search2
    //search

    /// <summary>
    ///     Returns albums, artists and songs matching the given search criteria. Supports paging through the result.
    /// </summary>
    /// <param name="request">Search request options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/search2.view")]
    [Route("/rest/search2")]
    public Task<IActionResult> Search2Async(SearchRequest request, CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.SearchAsync(request, false, ApiRequest, cancellationToken));
    }


    /// <summary>
    ///     Returns albums, artists and songs matching the given search criteria. Supports paging through the result.
    /// </summary>
    /// <param name="request">Search request options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/search3.view")]
    [Route("/rest/search3")]
    public Task<IActionResult> Search3Async(SearchRequest request, CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.SearchAsync(request, true, ApiRequest, cancellationToken));
    }
}
