using System.Threading.Channels;

namespace Melodee.Common.MessageBus;

public class InMemoryEventBusPublisher<T>(ChannelWriter<Event<T>> bus) : IEventPublisher<T>
{
    public async ValueTask Publish(Event<T> @event, CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
        {
            return;
        }
        await bus.WriteAsync(@event, token).ConfigureAwait(false);
    }

    public ValueTask DisposeAsync()
    {
        bus.TryComplete();
        return ValueTask.CompletedTask;
    }
}
