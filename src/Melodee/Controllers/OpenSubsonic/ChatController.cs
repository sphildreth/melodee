using System.Net;
using Melodee.Common.Serialization;
using Melodee.Services;
using Melodee.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Controllers.OpenSubsonic;

public class ChatController(ISerializer serializer, EtagRepository etagRepository) : ControllerBase(etagRepository, serializer)
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
