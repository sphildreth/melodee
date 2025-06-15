using System.Text.Json;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Melodee.Blazor.Services;

public class LocalStorageService(ProtectedLocalStorage protectedLocalStorage) : ILocalStorageService
{
    public async Task RemoveItemAsync(string key)
    {
        await protectedLocalStorage.DeleteAsync(key);
    }

    public async Task<string?> GetItemAsStringAsync(string key)
    {
        try
        {
            var result = await protectedLocalStorage.GetAsync<string?>(key);
            if (!result.Success || result.Value == null)
            {
                return null;
            }

            return result.Value;
        }
        catch
        {
            return null;
        }
    }

    public async Task SetItemAsStringAsync(string key, string value)
    {
        await protectedLocalStorage.SetAsync(key, value);
    }

    public async Task<T?> GetItem<T>(string key)
    {
        var json = await protectedLocalStorage.GetAsync<string?>(key);
        if (!json.Success || json.Value == null)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(json.Value);
    }

    public async Task SetItem<T>(string key, T value)
    {
        await protectedLocalStorage.SetAsync(key, JsonSerializer.Serialize(value));
    }
}
