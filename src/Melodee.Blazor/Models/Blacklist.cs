namespace Melodee.Blazor.Models;

#pragma warning disable CS8618
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
