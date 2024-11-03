using System.Diagnostics;
using Melodee.Common.Constants;
using Melodee.Common.Extensions;
using Melodee.Common.Serialization;
using Melodee.Common.Utility;

namespace Melodee.Common.Configuration;

/// <summary>
/// Configuration of Melodee system.
/// </summary>
/// <param name="Configuration">Initial configuration from database.</param>
public record MelodeeConfiguration(Dictionary<string, object?> Configuration) : IMelodeeConfiguration
{
    public static string FormattingDateTimeDisplayActivityFormatDefault = "HH:mm:ss.fff";
    
    public void SetSetting<T>(string key, T? value) => Configuration[key] = value;

    public T? GetValue<T>(string key, Func<T?, T?>? returnValue = null) => returnValue == null ? GetSettingValue<T>(Configuration, key) : returnValue(GetSettingValue<T>(Configuration, key));
    
    /// <summary>
    /// This return all known settings in the SettingsRegistry with the option to set up given values.
    /// </summary>
    /// <param name="settings">Optional collection of settings to set for result</param>
    /// <returns>All known Settings in SettingsRegistry</returns>
    public static Dictionary<string, object?> AllSettings(Dictionary<string, object?>? settings = null)
    {
        var result = new Dictionary<string, object?>();
        foreach (var settingName in typeof(SettingRegistry).GetAllPublicConstantValues<string>())
        {
            result.TryAdd(settingName, null);
        }
        if (settings != null)
        {
            foreach (var setting in settings)
            {
                if (result.ContainsKey(setting.Key))
                {
                    result[setting.Key] = setting.Value;
                }
            }
        }
        return result;
    }

    public static T? GetSettingValue<T>(Dictionary<string, object?> settings, string settingName, T? defaultValue = default)
    {
        if (settings.TryGetValue(settingName, out var setting))
        {
            try
            {
                return SafeParser.ChangeType<T>(setting);
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Error converting setting [{ settingName }]: {e.Message}]");
            }
        }
        return defaultValue;
    }

    public static bool IsTrue(Dictionary<string, object?> settings, string settingName)
    {
        if (settings.TryGetValue(settingName, out var setting))
        {
            try
            {
                return SafeParser.ToBoolean(setting);
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Error converting setting [{ settingName }]: {e.Message}]");
            }
        }
        return false;
    }
    
    public static void SetSetting(Dictionary<string, object?> settings, string key, object? value)
    {
        if (settings.ContainsKey(key))
        {
            settings[key] = value;
        }
        else
        {
            settings.TryAdd(key, value);
        }
    }    
    
    public static Dictionary<string, string[]> FromSerializedJsonDictionary(object? o, ISerializer serializer)
    {
        if (o == null)
        {
            return [];
        }
        var ss = SafeParser.ToString(o);
        if (ss.Nullify() == null)
        {
            return [];
        }
        return serializer.Deserialize<Dictionary<string, string[]>>(ss.Replace("'", "\"")) ?? new Dictionary<string, string[]>();
    }

    public static string[] FromSerializedJsonArray(object? o, ISerializer serializer)
    {
        if (o == null)
        {
            return [];
        }
        var ss = SafeParser.ToString(o);
        if (ss.Nullify() == null)
        {
            return [];
        }
        return serializer.Deserialize<string[]>(ss.Replace("'", "\"")) ?? Array.Empty<string>();
    }    
}
