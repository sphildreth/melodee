namespace Melodee.Common.Configuration;

public interface IMelodeeConfiguration
{
    Dictionary<string, object?> Configuration { get; }
}
