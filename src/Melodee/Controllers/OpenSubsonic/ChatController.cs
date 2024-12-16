using Melodee.Common.Serialization;
using Melodee.Services;
using Melodee.Utils;

namespace Melodee.Controllers.OpenSubsonic;

public class ChatController(ISerializer serializer, EtagRepository etagRepository, OpenSubsonicApiService openSubsonicApiService) : ControllerBase(etagRepository, serializer)
{
    //TODO
    // getChatMessages
    // addChatMessage
}
