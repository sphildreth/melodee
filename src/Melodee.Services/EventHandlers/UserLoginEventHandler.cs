using Melodee.Common.Data;
using Melodee.Common.MessageBus;
using Melodee.Common.MessageBus.Events;
using Melodee.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;
using Serilog.Events;
using SerilogTimings;

namespace Melodee.Services.EventHandlers;

public sealed class UserLoginEventHandler(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory) : IEventHandler<UserLoginEvent>
{
    public async ValueTask Handle(UserLoginEvent? eventData, CancellationToken cancellationToken = default)
    {
        if (eventData != null)
        {
            using (Operation.At(LogEventLevel.Debug).Time("[{HandlerName}]: Data [{EventData}]", nameof(UserLoginEventHandler), eventData.ToString()))
            {
                var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
                await using (var scopedContext = await contextFactory.CreateDbContextAsync(cancellationToken))
                {
                    await scopedContext.Users
                        .Where(x => x.Id == eventData.UserId)
                        .ExecuteUpdateAsync(setters =>
                            setters.SetProperty(x => x.LastActivityAt, now)
                                .SetProperty(x => x.LastLoginAt, now), cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
