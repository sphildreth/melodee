using System.Diagnostics;
using System.Web;
using Melodee.Common.Constants;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Serialization;
using Melodee.Common.Utility;

namespace Melodee.Common.Configuration;

/// <summary>
///     Configuration of Melodee system.
/// </summary>
/// <param name="Configuration">Initial configuration from database.</param>
public record MelodeeConfiguration(Dictionary<string, object?> Configuration) : IMelodeeConfiguration
{
    public const int SongFileNameNumberPadding = 4;

    public const int ImageNameNumberPadding = 2;

    public const string RequiredNotSetValue = "** REQUIRED: THIS MUST BE EDITED **";

    public const string FormattingDateTimeDisplayActivityFormatDefault = @"hh\:mm\:ss\.ffff";

    public const int BatchSizeDefault = 250;

    public const int BatchSizeMaximum = 1000;

    public const string DefaultNoValuePlaceHolder = "---";


    public string? RemoveUnwantedArticles(string? input)
    {
        if (input == null)
        {
            return null;
        }

        var ignoredArticles = GetValue<string>(SettingRegistry.ProcessingIgnoredArticles);
        if (ignoredArticles.Nullify() != null)
        {
            foreach (var ignoredArticle in ignoredArticles!.ToTags() ?? [])
            {
                var ia = $"{ignoredArticle} ";
                if (input.StartsWith(ia, StringComparison.OrdinalIgnoreCase))
                {
                    input = input.Replace(ia, string.Empty, StringComparison.OrdinalIgnoreCase);
                }
            }
        }

        return input.Nullify();
    }

    public string GenerateWebSearchUrl(object[] searchTerms)
    {
        var term = HttpUtility.UrlEncode(string.Join(" ", searchTerms));
        return $"https://www.google.com/search?q={term}";
    }

    public string GenerateImageUrl(string apiKey, ImageSize imageSize)
    {
        var baseUrl = GetValue<string>(SettingRegistry.SystemBaseUrl);
        if (baseUrl.Nullify() == null || baseUrl == RequiredNotSetValue)
        {
            throw new Exception($"Configuration setting [{SettingRegistry.SystemBaseUrl}] is invalid.");
        }

        return $"{baseUrl}/images/{apiKey}/{imageSize.ToString().ToLower()}";
    }

    public void SetSetting<T>(string key, T? value)
    {
        Configuration[key] = value;
    }

    public T? GetValue<T>(string key, Func<T?, T?>? returnValue = null)
    {
        return returnValue == null
            ? GetSettingValue<T>(Configuration, key)
            : returnValue(GetSettingValue<T>(Configuration, key));
    }

    public int BatchProcessingSize()
    {
        return GetValue<int?>(SettingRegistry.DefaultsBatchSize, i =>
        {
            if (i is null or < 1)
            {
                return BatchSizeDefault;
            }

            if (i > BatchSizeMaximum)
            {
                return BatchSizeMaximum;
            }

            return BatchSizeDefault;
        }) ?? BatchSizeDefault;
    }

    /// <summary>
    ///     This return all known settings in the SettingsRegistry with the option to set up given values.
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
                if (setting is null)
                {
                    return defaultValue;
                }

                return SafeParser.ChangeType<T?>(setting);
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Error converting setting [{settingName}]: {e.Message}]");
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
                Trace.WriteLine($"Error converting setting [{settingName}]: {e.Message}]");
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

    public static string[] FromSerializedJsonArrayNormalized(object? o, ISerializer serializer)
    {
        return FromSerializedJsonArray(o, serializer).Select(r => r.ToNormalizedString() ?? r).ToArray();
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

        return serializer.Deserialize<string[]>(ss.Replace("'", "\"")) ?? [];
    }
}
