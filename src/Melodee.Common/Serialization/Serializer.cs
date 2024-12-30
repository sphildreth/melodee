using System.Collections;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Extensions;
using Melodee.Common.Models.OpenSubsonic;
using Melodee.Common.Models.OpenSubsonic.Responses;
using Melodee.Common.Serialization.Convertors;
using Serilog;

namespace Melodee.Common.Serialization;

public sealed class Serializer(ILogger logger) : ISerializer
{
    private ILogger Logger { get; } = logger;

    public string? SerializeOpenSubsonicModelToXml(ResponseModel? model)
    {
        if (model == null)
        {
            return null;
        }
        var result = new StringBuilder($"<subsonic-response xmlns=\"https://subsonic.org/restapi\" status=\"{( model.IsSuccess ? "ok" : "error") }\" type=\"melodee\" version=\"{model.ResponseData.Version}\" serverVersion=\"{ model.ResponseData.ServerVersion}\" openSubsonic=\"true\">");

        if (model.ResponseData.Error != null)
        {
            result.Append($"<error code=\"{ model.ResponseData.Error.Code}\" message=\"{ model.ResponseData.Error.Message}\"/>");
        }
        else
        {
            if (model.ResponseData.Data != null)
            {
                if (model.ResponseData.DataPropertyName.Nullify() != null)
                {
                    result.Append($"<{ model.ResponseData.DataPropertyName}>");
                }

                if (model.ResponseData.Data is IEnumerable data)
                {
                    foreach(var element in data)
                    {
                        result.Append(((IOpenSubsonicToXml)element).ToXml()); 
                    }
                }
                else
                {
                    result.Append(((IOpenSubsonicToXml)model.ResponseData.Data).ToXml(model.ResponseData.DataPropertyName));    
                }
                if (model.ResponseData.DataPropertyName.Nullify() != null)
                {
                    result.Append($"</{ model.ResponseData.DataPropertyName}>");
                }                
            }
        }
        result.Append("</subsonic-response>");
        return result.ToString();
    }
    
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
