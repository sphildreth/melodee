namespace Melodee.Common.Serialization;

public interface ISerializer
{
    string? Serialize(object o);

    TOut? Deserialize<TOut>(string? s);
}
