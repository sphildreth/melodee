using System.Net;

namespace Melodee.Blazor.Services;

/// <summary>
///     Interface for the blacklist service
/// </summary>
public interface IBlacklistService
{
    /// <summary>
    ///     Checks if an email is blacklisted
    /// </summary>
    /// <param name="email">Email to check</param>
    /// <returns>True if email is blacklisted, otherwise false</returns>
    Task<bool> IsEmailBlacklistedAsync(string email);

    /// <summary>
    ///     Checks if an IP address is blacklisted
    /// </summary>
    /// <param name="ipAddress">IP address to check</param>
    /// <returns>True if IP address is blacklisted, otherwise false</returns>
    Task<bool> IsIpBlacklistedAsync(string ipAddress);

    /// <summary>
    ///     Checks if an IP address is blacklisted
    /// </summary>
    /// <param name="ipAddress">IP address to check</param>
    /// <returns>True if IP address is blacklisted, otherwise false</returns>
    Task<bool> IsIpBlacklistedAsync(IPAddress ipAddress);
}
