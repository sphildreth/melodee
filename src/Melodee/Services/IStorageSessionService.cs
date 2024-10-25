namespace Melodee.Services;

public interface IStorageSessionService
{
    Task<string> GetItemAsStringAsync(string key);
    Task SetItemAsStringAsync(string key, string value);
    Task RemoveItemAsync(string key);
}
