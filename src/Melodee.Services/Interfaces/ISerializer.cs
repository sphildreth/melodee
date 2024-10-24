namespace Melodee.Services.Interfaces;

public interface ISerializer
{
    string? Serialize(object o);

    TOut? Deserialize<TOut>(string? s);
}
