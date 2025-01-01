namespace Melodee.Common.MessageBus.Events;

public sealed record UserLoginEvent(int UserId, string UserName);
