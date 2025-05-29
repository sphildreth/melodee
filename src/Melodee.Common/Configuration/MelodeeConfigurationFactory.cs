using System.Collections;
using Melodee.Common.Data;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Configuration;

public sealed class MelodeeConfigurationFactory(IDbContextFactory<MelodeeDbContext> contextFactory)
    : IMelodeeConfigurationFactory
{
    private IMelodeeConfiguration? _configuration;
    private static readonly Lazy<Dictionary<string, object?>> EnvironmentVariables = new(() =>
        Environment.GetEnvironmentVariables()
            .Cast<DictionaryEntry>()
            .ToDictionary(
                entry => entry.Key.ToString()!, 
                entry => entry.Value,
                StringComparer.OrdinalIgnoreCase));

    public void Reset()
    {
        _configuration = null;
    }
    
    public static bool IsSetViaEnvironmentVariable(string key) => EnvironmentVariablesSettings().ContainsKey(key);

    public static Dictionary<string, object?> EnvironmentVariablesSettings() => EnvironmentVariables.Value;

    
    public static Dictionary<string, object?> UpdateWithEnvironmentVariables(Dictionary<string, object?> settings)
    {
        var allEnvVars = EnvironmentVariablesSettings();
        foreach (var (key, value) in allEnvVars)
        {
            settings[key] = value;
        }
        return settings;
    }
    
    public async Task<IMelodeeConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default)
    {
        if (_configuration == null)
        {
            await using (var scopedContext = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var settings = await scopedContext
                    .Settings
                    .ToDictionaryAsync(
                        x => x.Key, 
                        object? (x) => x.Value, 
                        comparer: StringComparer.OrdinalIgnoreCase,
                        cancellationToken)
                    .ConfigureAwait(false);

                _configuration = new MelodeeConfiguration(UpdateWithEnvironmentVariables(settings));
            }
        }

        return _configuration!;
    }
}
