using Melodee.Common.Data;
using Melodee.Common.MessageBus.Events;
using Melodee.Common.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Rebus.Handlers;
using Serilog;

namespace Melodee.Common.MessageBus.EventHandlers;

public sealed class AlbumUpdatedEventHandler(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory) : IHandleMessages<AlbumUpdatedEvent>
{

    public Task Handle(AlbumUpdatedEvent message)
    {
        // TODO update db or something        
        Console.WriteLine($"[{nameof(AlbumUpdatedEventHandler)}]: {message}");            

        return Task.CompletedTask;
    }
}
