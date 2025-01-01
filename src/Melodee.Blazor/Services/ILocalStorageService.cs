namespace Melodee.Blazor.Services;

public interface ILocalStorageService
{
    Task<string?> GetItemAsStringAsync(string key);
    
    Task SetItemAsStringAsync(string key, string value);
    
    Task RemoveItemAsync(string key);    
}
