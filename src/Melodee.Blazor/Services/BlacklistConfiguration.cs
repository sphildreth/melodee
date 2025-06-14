namespace Melodee.Blazor.Services;

/// <summary>
///     Configuration for the blacklist service
/// </summary>
public class BlacklistConfiguration
{
    /// <summary>
    ///     List of blacklisted email addresses
    /// </summary>
    public List<string> BlacklistedEmails { get; set; } = new();

    /// <summary>
    ///     List of blacklisted IP addresses
    /// </summary>
    public List<string> BlacklistedIPs { get; set; } = new();
}
