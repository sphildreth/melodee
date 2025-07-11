using System.Net;
using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Models.OpenSubsonic.Requests;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Blazor.Controllers.OpenSubsonic;

public class UserManagementController(ISerializer serializer, EtagRepository etagRepository, OpenSubsonicApiService openSubsonicApiService, IMelodeeConfigurationFactory configurationFactory) : ControllerBase(etagRepository, serializer, configurationFactory)
{
    [HttpGet]
    [HttpPost]
    [Route("/rest/updateUser.view")]
    [Route("/rest/updateUser")]
    [Route("/rest/deleteUser.view")]
    [Route("/rest/deleteUser")]
    [Route("/rest/changePassword.view")]
    [Route("/rest/changePassword")]
    [Route("/rest/getUsers.view")]
    [Route("/rest/getUsers")]    
    public IActionResult NotImplemented()
    {
        HttpContext.Response.Headers.Append("Cache-Control", "no-cache");
        return StatusCode((int)HttpStatusCode.NotImplemented);
    }

    /// <summary>
    ///     Get details about a given user, including which authorization roles and folder access it has.
    /// </summary>
    /// <param name="username">
    ///     The name of the user to retrieve. You can only retrieve your own user unless you have admin
    ///     privileges.
    /// </param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getUser.view")]
    [Route("/rest/getUser")]
    public Task<IActionResult> GetUserAsync(string username, CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.GetUserAsync(username, ApiRequest, cancellationToken));
    }

    /// <summary>
    ///     Creates a new user on the server.
    /// </summary>
    /// <param name="request">Populated model from Query parameters.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/createUser.view")]
    [Route("/rest/createUser")]
    public Task<IActionResult> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.CreateUserAsync(request, ApiRequest, cancellationToken));
    }
}
