namespace Melodee.Common.MessageBus;

public sealed class EventContextAccessor<T> : IEventContextAccessor<T>
{
    private static readonly AsyncLocal<EventMetadataWrapper<T>> Holder = new();

    public Event<T>? Event => Holder.Value?.Event;

    public void Set(Event<T> @event)
    {
        var holder = Holder.Value;
        if (holder != null)
        {
            holder.Event = null;
        }

        Holder.Value = new EventMetadataWrapper<T> { Event = @event };
    }
}

public sealed class EventMetadataWrapper<T>
{
    public Event<T>? Event { get; set; }
}
