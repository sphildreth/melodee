using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Blazor.Controllers.OpenSubsonic;

public class SharingController(
    ISerializer serializer,
    EtagRepository etagRepository,
    OpenSubsonicApiService openSubsonicApiService,
    IMelodeeConfigurationFactory configurationFactory) : ControllerBase(etagRepository,
    serializer,
    configurationFactory)
{
   
    /// <summary>
    ///     Returns information about shared media this user is allowed to manage.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getShares.view")]
    [Route("/rest/getShares")]
    public Task<IActionResult> GetSharesAsync(CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.GetSharesAsync(ApiRequest, cancellationToken));
    }    
    
    /// <summary>
    ///     Deletes an existing share.
    /// </summary>
    /// <param name="id">ID of the share to delete.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/deleteShare.view")]
    [Route("/rest/deleteShare")]
    public Task<IActionResult> DeleteShareAsync(string id, CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.DeleteShareAsync(id, ApiRequest, cancellationToken));
    }    
    
    /// <summary>
    ///     Creates a public URL that can be used by anyone to stream music or playlist from the server.
    /// </summary>
    /// <param name="id">ID of a song, album or playlist to share. Use one id parameter for each entry to share.</param>
    /// <param name="description">A user-defined description that will be displayed to people visiting the shared media.</param>
    /// <param name="expires">The time at which the share expires. Given as milliseconds since 1970.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/createShare.view")]
    [Route("/rest/createShare")]
    public Task<IActionResult> CreateShareAsync(string id, string? description, long? expires, CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.CreateShareAsync(ApiRequest, id, description, expires, cancellationToken));
    }
    
    /// <summary>
    ///     Updates the description and/or expiration date for an existing share.
    /// </summary>
    /// <param name="id">ID of the share to update.</param>
    /// <param name="description">A user-defined description that will be displayed to people visiting the shared media.</param>
    /// <param name="expires">The time at which the share expires. Given as milliseconds since 1970.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/updateShare.view")]
    [Route("/rest/updateShare")]
    public Task<IActionResult> UpdateShareAsync(string id, string? description, long? expires, CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.UpdateShareAsync(ApiRequest, id, description, expires, cancellationToken));
    }      
}
