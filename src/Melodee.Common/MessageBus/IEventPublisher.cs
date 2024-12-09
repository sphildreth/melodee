namespace Melodee.Common.MessageBus;

public interface IEventPublisher<T> : IAsyncDisposable
{
    ValueTask Publish(Event<T> @event, CancellationToken token = default);
}
