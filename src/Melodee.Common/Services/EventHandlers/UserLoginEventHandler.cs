using Melodee.Common.MessageBus;
using Melodee.Common.MessageBus.Events;

namespace Melodee.Common.Services.EventHandlers;

public sealed class UserLoginEventHandler(UserService userService) : IEventHandler<UserLoginEvent>
{
    public Task Handle(UserLoginEvent? eventData, CancellationToken cancellationToken = default)
    {
        if (eventData != null)
        {
            return userService.UpdateLastLogin(eventData, cancellationToken);
        }
        return Task.CompletedTask;
    }
}
