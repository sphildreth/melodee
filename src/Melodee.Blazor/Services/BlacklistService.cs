using System.Net;
using Melodee.Blazor.Models;
using Microsoft.Extensions.Options;

namespace Melodee.Blazor.Services
{
    /// <summary>
    /// Service for checking if a user's email or IP address is blacklisted
    /// </summary>
    public sealed class BlacklistService : IBlacklistService
    {
        private readonly MelodeeAppSettingsConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlacklistService"/> class.
        /// </summary>
        /// <param name="configuration">The blacklist configuration</param>
        public BlacklistService(IOptions<MelodeeAppSettingsConfiguration> configuration)
        {
            _configuration = configuration.Value;
        }

        /// <summary>
        /// Checks if an email is blacklisted
        /// </summary>
        /// <param name="email">Email to check</param>
        /// <returns>True if email is blacklisted, otherwise false</returns>
        public Task<bool> IsEmailBlacklistedAsync(string email)
        {
            if (_configuration.Blacklist.BlacklistedEmails == null || _configuration.Blacklist.BlacklistedEmails.Length == 0 || string.IsNullOrWhiteSpace(email))
            {
                return Task.FromResult(false);
            }

            string normalizedEmail = email.Trim().ToLowerInvariant();
            return Task.FromResult(_configuration.Blacklist.BlacklistedEmails!.Contains(normalizedEmail));
        }

        /// <summary>
        /// Checks if an IP address is blacklisted
        /// </summary>
        /// <param name="ipAddress">IP address to check</param>
        /// <returns>True if IP address is blacklisted, otherwise false</returns>
        public Task<bool> IsIpBlacklistedAsync(string ipAddress)
        {
            if (_configuration.Blacklist.BlacklistedIPs == null || _configuration.Blacklist.BlacklistedIPs.Length == 0 || string.IsNullOrWhiteSpace(ipAddress))
            {
                return Task.FromResult(false);
            }

            string normalizedIp = ipAddress.Trim();
            return Task.FromResult(_configuration.Blacklist.BlacklistedIPs!.Contains(normalizedIp));
        }

        /// <summary>
        /// Checks if an IP address is blacklisted
        /// </summary>
        /// <param name="ipAddress">IP address to check</param>
        /// <returns>True if IP address is blacklisted, otherwise false</returns>
        public Task<bool> IsIpBlacklistedAsync(IPAddress ipAddress)
        {
            if (ipAddress == null)
            {
                return Task.FromResult(false);
            }
            return IsIpBlacklistedAsync(ipAddress.ToString());
        }
    }
}
