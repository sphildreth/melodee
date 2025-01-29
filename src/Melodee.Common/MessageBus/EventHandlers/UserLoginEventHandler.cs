using Melodee.Common.MessageBus.Events;
using Melodee.Common.Services;
using Rebus.Handlers;

namespace Melodee.Common.MessageBus.EventHandlers;

public sealed class UserLoginEventHandler(UserService userService) : IHandleMessages<UserLoginEvent>
{
    public Task Handle(UserLoginEvent eventData)
    {
        return userService.UpdateLastLogin(eventData);
    }
}
