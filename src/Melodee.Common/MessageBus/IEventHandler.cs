namespace Melodee.Common.MessageBus;

public interface IEventHandler<in T>
{
    Task Handle(T? eventData, CancellationToken cancellationToken = default);
}
