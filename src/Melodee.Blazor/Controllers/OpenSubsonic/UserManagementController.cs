using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Models.OpenSubsonic.Requests;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Blazor.Controllers.OpenSubsonic;

public class UserManagementController(ISerializer serializer, EtagRepository etagRepository, OpenSubsonicApiService openSubsonicApiService, IMelodeeConfigurationFactory configurationFactory) : ControllerBase(etagRepository, serializer, configurationFactory)
{
    //TODO
    // getUsers

    // updateUser
    // deleteUser
    // changePassword


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
