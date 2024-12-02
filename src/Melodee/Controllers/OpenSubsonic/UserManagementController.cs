using Melodee.Common.Models.OpenSubsonic.Requests;
using Melodee.Common.Serialization;
using Melodee.Results;
using Melodee.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Controllers.OpenSubsonic;

public class UserManagementController(ISerializer serializer, OpenSubsonicApiService openSubsonicApiService) : ControllerBase
{
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
    public async Task<IActionResult> GetUserAsync(string username, CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.GetUserAsync(username, ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }

    /// <summary>
    ///     Creates a new user on the server.
    /// </summary>
    /// <param name="request">Populated model from Query parameters.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/createUser.view")]
    public async Task<IActionResult> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.CreateUserAsync(request, ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }
}
