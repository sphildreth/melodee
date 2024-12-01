using Melodee.Common.Enums;

namespace Melodee.Common.Configuration;

public interface IMelodeeConfiguration
{
    string GetBuildImageUrl(string apiKey, ImageSize imageSize);
    
    T? GetValue<T>(string key, Func<T?, T>? returnValue = null);

    int BatchProcessingSize();

    void SetSetting<T>(string key, T? value);
    
    Dictionary<string, object?> Configuration { get; }
}
