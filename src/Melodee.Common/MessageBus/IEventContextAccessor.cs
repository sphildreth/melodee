namespace Melodee.Common.MessageBus;

public interface IEventContextAccessor<T>
{
    public Event<T>? Event { get; }
    void Set(Event<T> @event);
}
