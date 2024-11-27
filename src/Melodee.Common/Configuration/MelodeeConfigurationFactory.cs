using Melodee.Common.Data;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Configuration;

public sealed class MelodeeConfigurationFactory(IDbContextFactory<MelodeeDbContext> contextFactory) : IMelodeeConfigurationFactory
{
    public async Task<IMelodeeConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default)
    {
        await using (var scopedContext = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var settings = await scopedContext
                .Settings
                .ToDictionaryAsync(x => x.Key, object? (x) => x.Value, cancellationToken)
                .ConfigureAwait(false);
            return new MelodeeConfiguration(settings);            
        }
    }
}
