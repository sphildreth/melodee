using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Blazor.Controllers.OpenSubsonic;

public class InternetRadioController(
    ISerializer serializer,
    EtagRepository etagRepository,
    OpenSubsonicApiService openSubsonicApiService,
    IMelodeeConfigurationFactory configurationFactory) : ControllerBase(etagRepository,
    serializer,
    configurationFactory)
{
    // 

    /// <summary>
    ///     Deletes an existing internet radio station.
    /// </summary>
    /// <param name="id">ID of the radio station to delete, as obtained by getInternetRadioStations.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/deleteInternetRadioStation.view")]
    [Route("/rest/deleteInternetRadioStation")]
    public Task<IActionResult> DeleteInternetRadioStationAsync(string id, CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.DeleteInternetRadioStationAsync(id, ApiRequest, cancellationToken));
    }

    /// <summary>
    ///     Adds a new internet radio station.
    /// </summary>
    /// <param name="name">The stream URL for the station.</param>
    /// <param name="streamUrl">The user-defined name for the station.</param>
    /// <param name="homePageUrl">The home page URL for the station.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/createInternetRadioStation.view")]
    [Route("/rest/createInternetRadioStation")]
    public Task<IActionResult> CreateInternetRadioStationAsync(string name, string streamUrl, string? homePageUrl, CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.CreateInternetRadioStationAsync(name, streamUrl, homePageUrl, ApiRequest, cancellationToken));
    }

    /// <summary>
    ///     Updates an existing internet radio station.
    /// </summary>
    /// <param name="id">ID of the radio station to update, as obtained by getInternetRadioStations.</param>
    /// <param name="name">The stream URL for the station.</param>
    /// <param name="streamUrl">The user-defined name for the station.</param>
    /// <param name="homePageUrl">The home page URL for the station.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/updateInternetRadioStation.view")]
    [Route("/rest/updateInternetRadioStation")]
    public Task<IActionResult> UpdateInternetRadioStationAsync(string id, string name, string streamUrl, string? homePageUrl, CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.UpdateInternetRadioStationAsync(id, name, streamUrl, homePageUrl, ApiRequest, cancellationToken));
    }

    /// <summary>
    ///     Returns all internet radio stations.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getInternetRadioStations.view")]
    [Route("/rest/getInternetRadioStations")]
    public Task<IActionResult> GetInternetRadioStationsAsync(CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.GetInternetRadioStationsAsync(ApiRequest, cancellationToken));
    }
}
