using System.Text.Json;
using Serilog;

namespace Melodee.Common.Serialization;

public sealed class Serializer(ILogger logger) : ISerializer
{
    private ILogger Logger { get; } = logger;

    public string? Serialize(object? o)
    {
        if (o == null)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Serialize(o);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to Serialize [{0}]", o.ToString());
        }

        return null;
    }

    public TOut? Deserialize<TOut>(string? s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<TOut>(s);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to Deserialize [{0}]", s);
        }

        return default;
    }
}
