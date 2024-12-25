using Melodee.Common.Data;
using Melodee.Common.MessageBus;
using Melodee.Common.MessageBus.Events;
using Melodee.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Melodee.Services.EventHandlers;

public sealed class AlbumUpdatedEventHandler (
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory) : IEventHandler<AlbumUpdatedEvent>
{
    public Task Handle(AlbumUpdatedEvent? eventData, CancellationToken cancellationToken = default)
    {
        if (eventData != null)
        {
            // TODO update db or something
        }
        return Task.CompletedTask;
    }
}
