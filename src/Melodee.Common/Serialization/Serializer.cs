using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        => Deserialize<TOut>(Encoding.UTF8.GetBytes(s ?? string.Empty));

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    
    public TOut? Deserialize<TOut>(byte[]? bytes)
    {
        if (bytes?.Length == 0)
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<TOut>(bytes, _jsonSerializerOptions);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to Deserialize [{0}]", typeof(TOut).Name);
        }

        return default;
    }
}
