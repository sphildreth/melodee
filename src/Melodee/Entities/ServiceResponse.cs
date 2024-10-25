namespace Melodee.Entities;

public class ServiceResponse<T>(string message, bool success = false, T? data = default(T?))
{
    public T? Data { get; set; } = data;
    public bool Success { get; set; } = success;
    public string Message { get; set; } = message;
}
