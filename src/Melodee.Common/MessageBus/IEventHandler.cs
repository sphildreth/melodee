namespace Melodee.Common.MessageBus;

public interface IEventHandler<in T>
{
    ValueTask Handle(T? eventData, CancellationToken cancellationToken = default);
}
