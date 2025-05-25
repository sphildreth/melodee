#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
namespace Melodee.Blazor.Models;

public class MelodeeAppSettingsConfiguration
{
    public MelodeeAppSettingsConfiguration()
    {
    }
    
    public MelodeeAppSettingsConfiguration(string baseUrl, Blacklist blacklist)
    {
        BaseUrl = baseUrl;
        Blacklist = blacklist;
    }

    public string BaseUrl { get; init; }
    public Blacklist Blacklist { get; init; }
}
