using System.Net;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Serialization;
using Microsoft.Extensions.Options;

namespace Melodee.Blazor.Services
{
    /// <summary>
    /// Service for checking if a user's email or IP address is blacklisted
    /// </summary>
    public sealed class BlacklistService : IBlacklistService
    {
        private readonly ISerializer _serializer;
        private readonly IMelodeeConfigurationFactory _configurationFactory;

       
        /// <summary>
        /// Initializes a new instance of the <see cref="BlacklistService"/> class.
        /// </summary>
        /// <param name="configuration">The blacklist configuration</param>
        public BlacklistService(ISerializer serializer, IMelodeeConfigurationFactory configurationFactory)
        {
            _serializer = serializer;
            _configurationFactory = configurationFactory;
        }

        /// <summary>
        /// Checks if an email is blacklisted
        /// </summary>
        /// <param name="email">Email to check</param>
        /// <returns>True if email is blacklisted, otherwise false</returns>
        public async Task<bool> IsEmailBlacklistedAsync(string email)
        {
            var configuration = await _configurationFactory.GetConfigurationAsync().ConfigureAwait(false);
            var blacklistedEmails = MelodeeConfiguration.FromSerializedJsonArray(configuration.GetValue<string>(SettingRegistry.SecurityBlacklistedEmails), _serializer);
            if (blacklistedEmails.Length == 0 || string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            string normalizedEmail = email.Trim().ToLowerInvariant();
            return blacklistedEmails.Contains(normalizedEmail);
        }

        /// <summary>
        /// Checks if an IP address is blacklisted
        /// </summary>
        /// <param name="ipAddress">IP address to check</param>
        /// <returns>True if IP address is blacklisted, otherwise false</returns>
        public async Task<bool> IsIpBlacklistedAsync(string ipAddress)
        {
            var configuration = await _configurationFactory.GetConfigurationAsync().ConfigureAwait(false);
            var blacklistedIPs = MelodeeConfiguration.FromSerializedJsonArray(configuration.GetValue<string>(SettingRegistry.SecurityBlacklistedIPs), _serializer);
            if (blacklistedIPs.Length == 0 || string.IsNullOrWhiteSpace(ipAddress))
            {
                return false;
            }

            string normalizedIp = ipAddress.Trim();
            return blacklistedIPs.Contains(normalizedIp);
        }

        /// <summary>
        /// Checks if an IP address is blacklisted
        /// </summary>
        /// <param name="ipAddress">IP address to check</param>
        /// <returns>True if IP address is blacklisted, otherwise false</returns>
        public Task<bool> IsIpBlacklistedAsync(IPAddress? ipAddress)
        {
            if (ipAddress == null)
            {
                return Task.FromResult(false);
            }
            return IsIpBlacklistedAsync(ipAddress.ToString());
        }
    }
}
