using Asp.Versioning;
using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Blazor.Controllers.Melodee;

[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class SearchController(
    ISerializer serializer,
    EtagRepository etagRepository,
    UserService userService,
    IConfiguration configuration,
    IMelodeeConfigurationFactory configurationFactory) : ControllerBase(
    etagRepository,
    serializer,
    configuration,
    configurationFactory)
{
    
    [HttpGet]
    [Route("search")]
    public Task<IActionResult> SearchAsync(string query, string? includes, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
