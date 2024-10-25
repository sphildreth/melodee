using Blazored.SessionStorage;

namespace Melodee.Services;

/// <summary>
///     A service to manage the browser session storage using Blazored.SessionStorage
/// </summary>
public class StorageSessionService(ISessionStorageService sessionService) : IStorageSessionService
{
    public async Task<string> GetItemAsStringAsync(string key)
    {
        return await sessionService.GetItemAsStringAsync(key);
    }

    public async Task SetItemAsStringAsync(string key, string value)
    {
        await sessionService.SetItemAsStringAsync(key, value);
    }

    public async Task RemoveItemAsync(string key)
    {
        await sessionService.RemoveItemAsync(key);
    }
}
