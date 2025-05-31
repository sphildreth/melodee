using Asp.Versioning;
using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Blazor.Controllers.Melodee;

/// <summary>
/// This controller is used to get meta-information about the API.
/// </summary>
[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class MetaController(
    ISerializer serializer,
    EtagRepository etagRepository,
    UserService userService,
    StatisticsService statisticsService,
    IConfiguration configuration,
    IMelodeeConfigurationFactory configurationFactory) : ControllerBase(
    etagRepository,
    serializer,
    configuration,
    configurationFactory)
{
    /// <summary>
    /// Return some statistics about the system.
    /// </summary>
    [HttpGet]
    [Route("stats")]
    public Task<IActionResult> GetSystemStatsAsync(CancellationToken cancellationToken = default)
    {
        
        throw new NotImplementedException();
    } 
}
