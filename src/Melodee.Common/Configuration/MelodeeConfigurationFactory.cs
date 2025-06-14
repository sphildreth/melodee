using System.Collections;
using System.Diagnostics;
using Melodee.Common.Data;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Configuration;

public sealed class MelodeeConfigurationFactory(IDbContextFactory<MelodeeDbContext> contextFactory)
    : IMelodeeConfigurationFactory
{
    private static readonly Lazy<Dictionary<string, object?>> EnvironmentVariables = new(() =>
        Environment.GetEnvironmentVariables()
            .Cast<DictionaryEntry>()
            .ToDictionary(
                entry => entry.Key.ToString()!,
                entry => entry.Value,
                StringComparer.OrdinalIgnoreCase));

    private IMelodeeConfiguration? _configuration;

    public void Reset()
    {
        _configuration = null;
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
                        StringComparer.OrdinalIgnoreCase,
                        cancellationToken)
                    .ConfigureAwait(false);

                _configuration = new MelodeeConfiguration(UpdateWithEnvironmentVariables(settings));
            }
        }

        return _configuration!;
    }

    public static bool IsSetViaEnvironmentVariable(string key)
    {
        return EnvironmentVariablesSettings().ContainsKey(key);
    }

    public static Dictionary<string, object?> EnvironmentVariablesSettings()
    {
        return EnvironmentVariables.Value;
    }


    public static Dictionary<string, object?> UpdateWithEnvironmentVariables(Dictionary<string, object?> settings)
    {
        var allEnvVars = EnvironmentVariablesSettings();
        foreach (var (key, value) in allEnvVars)
        {
            var kk = key.Replace("_", ".");
            if (settings.ContainsKey(kk) && settings[kk] != value)
            {
                settings[kk] = value;
                Trace.WriteLine($"[{nameof(MelodeeConfigurationFactory)}] Overriding setting [{kk}] with environment variable value [{value}]");
            }
            else
            {
                settings.Add(kk, value);
                Trace.WriteLine($"[{nameof(MelodeeConfigurationFactory)}] Added setting [{kk}] with environment variable value [{value}]");
            }
        }

        return settings;
    }
}
