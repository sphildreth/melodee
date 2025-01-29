using Melodee.Common.Data;
using Melodee.Common.MessageBus.Events;
using Microsoft.EntityFrameworkCore;
using Rebus.Handlers;
using Serilog;

namespace Melodee.Common.MessageBus.EventHandlers;

public sealed class SearchHistoryEventHandler(
    ILogger logger,
    IDbContextFactory<MelodeeDbContext> contextFactory) : IHandleMessages<SearchHistoryEvent>
{
    public async Task Handle(SearchHistoryEvent eventData)
    {
        logger.Debug("[{Name}]: {eventData}", nameof(SearchHistoryEventHandler), eventData);
        await using (var scopedContext = await contextFactory.CreateDbContextAsync().ConfigureAwait(false))
        {
            var user = await scopedContext.Users.FirstOrDefaultAsync(x => x.ApiKey == eventData.ByUserApiKey);
            if (user != null)
            {
                eventData.ByUserId = user.Id;
                scopedContext.SearchHistories.Add(eventData);
                await scopedContext.SaveChangesAsync();
            }
        }
    }
}
