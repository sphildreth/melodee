using System.Collections;
using Melodee.Common.Data;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Configuration;

public sealed class MelodeeConfigurationFactory(IDbContextFactory<MelodeeDbContext> contextFactory)
    : IMelodeeConfigurationFactory
{
    private IMelodeeConfiguration? _configuration;

    public void Reset()
    {
        _configuration = null;
    }

    public async Task<IMelodeeConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default)
    {
        if (_configuration == null)
        {
            await using (var scopedContext =
                         await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var settings = await scopedContext
                    .Settings
                    .ToDictionaryAsync(x => x.Key, object? (x) => x.Value, cancellationToken)
                    .ConfigureAwait(false);
                
                var allEnvVars = Environment.GetEnvironmentVariables()
                    .Cast<DictionaryEntry>()
                    .ToDictionary(entry => entry.Key.ToString(), entry => entry.Value?.ToString());

                foreach (var (key, value) in allEnvVars)
                {
                    if (key != null)
                    {
                        settings[key] = value;
                    }
                }
                _configuration = new MelodeeConfiguration(settings);
            }
        }

        return _configuration!;
    }
}
