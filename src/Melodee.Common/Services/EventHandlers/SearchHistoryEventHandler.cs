using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.MessageBus;
using Melodee.Common.MessageBus.Events;
using Melodee.Common.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Melodee.Common.Services.EventHandlers;

public sealed class SearchHistoryEventHandler
(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory) : IEventHandler<SearchHistoryEvent>
{
    public async Task Handle(SearchHistoryEvent? eventData, CancellationToken cancellationToken = default)
    {
        if (eventData == null)
        {
            return;
        }
        await using (var scopedContext = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var user = await scopedContext.Users.FirstOrDefaultAsync(x => x.ApiKey == eventData.ByUserApiKey, cancellationToken: cancellationToken);
            if (user != null)
            {
                eventData.ByUserId = user.Id;
                scopedContext.SearchHistories.Add(eventData);
                await scopedContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
