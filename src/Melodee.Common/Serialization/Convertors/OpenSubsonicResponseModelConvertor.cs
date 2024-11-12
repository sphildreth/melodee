using System.Text.Json;
using System.Text.Json.Serialization;
using Melodee.Common.Models.OpenSubsonic.Responses;

namespace Melodee.Common.Serialization.Convertors;

public class OpenSubsonicResponseModelConvertor : JsonConverter<ResponseModel>
{
    public override ResponseModel? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, ResponseModel value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("subsonic-response");

        writer.WriteStartObject();

        writer.WritePropertyName("status");
        writer.WriteStringValue(value.ResponseData.Status);
        writer.WritePropertyName("version");
        writer.WriteStringValue(value.ResponseData.Version);
        writer.WritePropertyName("type");
        writer.WriteStringValue(value.ResponseData.Type);
        writer.WritePropertyName("serverVersion");
        writer.WriteStringValue(value.ResponseData.ServerVersion);
        writer.WritePropertyName("openSubsonic");
        writer.WriteBooleanValue(true);

        if (!string.IsNullOrEmpty(value.ResponseData.DataPropertyName))
        {
            writer.WritePropertyName(value.ResponseData.DataPropertyName);
        }

        var hasDetailPropertyName = !string.IsNullOrEmpty(value.ResponseData.DataDetailPropertyName);

        if (hasDetailPropertyName)
        {
            writer.WriteStartObject();
        }
        if (value.ResponseData.Data != null)
        {
            if (hasDetailPropertyName)
            {
                writer.WritePropertyName(value.ResponseData.DataDetailPropertyName);
            }
            writer.WriteRawValue(JsonSerializer.Serialize(value.ResponseData.Data, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            }));
        }

        if (hasDetailPropertyName)
        {
            writer.WriteEndObject();
        }

        writer.WriteEndObject();

        writer.WriteEndObject();
    }
}
