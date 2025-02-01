using System.Diagnostics;
using Melodee.Common.MessageBus.Events;
using Rebus.Handlers;

namespace Melodee.Common.MessageBus.EventHandlers;

public sealed class AlbumUpdatedEventHandler : IHandleMessages<AlbumUpdatedEvent>
{
    public Task Handle(AlbumUpdatedEvent message)
    {
        // TODO update db or something        
        Trace.WriteLine($"[{nameof(AlbumUpdatedEventHandler)}]: {message}");

        return Task.CompletedTask;
    }
}
