using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Melodee.Common.Serialization.Convertors;
using Serilog;
using Serilog.Core;

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
            return JsonSerializer.Serialize(o, JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to Serialize [{0}]", o.ToString());
            throw;
        }
    }
    
    public TOut? Deserialize<TOut>(string? s)
        => Deserialize<TOut>(Encoding.UTF8.GetBytes(s ?? string.Empty));

    public static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new OpenSubsonicResponseModelConvertor() }
    };
    
    public TOut? Deserialize<TOut>(byte[]? bytes)
    {
        if (bytes?.Length == 0)
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<TOut>(bytes, JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to Deserialize [{0}]", typeof(TOut).Name);
            throw;
        }
    }
}
