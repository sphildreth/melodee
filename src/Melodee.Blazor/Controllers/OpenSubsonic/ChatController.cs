using System.Net;
using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Blazor.Controllers.OpenSubsonic;

public class ChatController(ISerializer serializer, EtagRepository etagRepository,IMelodeeConfigurationFactory configurationFactory) : ControllerBase(etagRepository, serializer, configurationFactory)
{
    [HttpGet]
    [HttpPost]
    [Route("/rest/getChatMessages.view")]
    [Route("/rest/addChatMessage.view")]
    [Route("/rest/getChatMessages")]
    [Route("/rest/addChatMessage")]    
    public IActionResult DeprecatedWontImplement()
    {
        HttpContext.Response.Headers.Append("Cache-Control", "no-cache");
        return StatusCode((int)HttpStatusCode.Gone);
    }     
}
