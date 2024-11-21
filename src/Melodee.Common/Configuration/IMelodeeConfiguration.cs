namespace Melodee.Common.Configuration;

public interface IMelodeeConfiguration
{
    T? GetValue<T>(string key, Func<T?, T>? returnValue = null);

    int BatchProcessingSize();

    void SetSetting<T>(string key, T? value);
    
    Dictionary<string, object?> Configuration { get; }
}
