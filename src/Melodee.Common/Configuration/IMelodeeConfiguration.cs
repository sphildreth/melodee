namespace Melodee.Common.Configuration;

public interface IMelodeeConfiguration
{
    T? GetValue<T>(string key);
    
    Dictionary<string, object?> Configuration { get; }
}
