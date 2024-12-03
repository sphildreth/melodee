using Melodee.Common.Serialization;
using Melodee.Services;

namespace Melodee.Controllers.OpenSubsonic;

public class ChatController(ISerializer serializer, OpenSubsonicApiService openSubsonicApiService) : ControllerBase(serializer)
{
    // getChatMessages
    // addChatMessage
}
