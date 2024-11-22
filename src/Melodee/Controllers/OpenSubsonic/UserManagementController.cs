using Melodee.Common.Models.OpenSubsonic;
using Melodee.Common.Models.OpenSubsonic.Requests;
using Melodee.Common.Models.OpenSubsonic.Responses;
using Melodee.Common.Serialization;
using Melodee.Results;
using Melodee.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Controllers.OpenSubsonic;

public class UserManagementController(ISerializer serializer, OpenSubsonicApiService openSubsonicApiService) : ControllerBase
{

    // getUser
    // getUsers
    // updateUser
    // deleteUser
    // changePassword

    /// <summary>
    /// Creates a new user on the server.
    /// </summary>
    /// <param name="request">Populated model from Query parameters.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("/rest/createUser.view")]
    public async Task<IActionResult> CreateUser(CreateUserRequest request, CancellationToken cancellationToken = default)
        => new JsonStringResult(serializer.Serialize(await openSubsonicApiService.CreateUserAsync(request, ApiRequest, cancellationToken).ConfigureAwait(false))!);

}
