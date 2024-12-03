using Melodee.Common.Configuration;
using Melodee.Common.Models.OpenSubsonic.Responses;

namespace Melodee.Common.Serialization;

public interface ISerializer
{
    string? Serialize(object? o);

    string? SerializeOpenSubsonicModelToXml(ResponseModel? model);

    TOut? Deserialize<TOut>(byte[]? s);
    
    TOut? Deserialize<TOut>(string? s);
}
