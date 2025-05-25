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

public class Blacklist
{
    public Blacklist()
    {
    }
    
    public Blacklist(string[]? blacklistedEmails, string[]? blacklistedIPs)
    {
        BlacklistedEmails = blacklistedEmails;
        BlacklistedIPs = blacklistedIPs;
    }

    public string[]? BlacklistedEmails { get; init; }
    public string[]? BlacklistedIPs { get; init; }

}
