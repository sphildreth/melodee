using System;
using System.Security.Cryptography;
using System.Text;

namespace Melodee.Common.Security
{
    public class HmacTokenService
    {
        private readonly string _secretKey;

        public HmacTokenService(string secretKey)
        {
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new ArgumentNullException(nameof(secretKey), "Secret key cannot be null or empty");
            }
            _secretKey = secretKey;
        }

        /// <summary>
        /// Generates an HMAC token for the provided data
        /// </summary>
        /// <param name="data">The data to create a token for</param>
        /// <returns>Base64 encoded HMAC token</returns>
        public string GenerateToken(string data)
        {
            if (string.IsNullOrEmpty(data))
                throw new ArgumentNullException(nameof(data), "Data cannot be null or empty");

            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secretKey)))
            {
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                byte[] hashBytes = hmac.ComputeHash(dataBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        /// <summary>
        /// Validates if the provided token matches the expected HMAC for the data
        /// </summary>
        /// <param name="data">The original data</param>
        /// <param name="token">The token to validate</param>
        /// <returns>True if token is valid, false otherwise</returns>
        public bool ValidateToken(string data, string token)
        {
            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentNullException(nameof(data), "Data cannot be null or empty");
            }

            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException(nameof(token), "Token cannot be null or empty");
            }
            string expectedToken = GenerateToken(data);
            
            // Use a constant-time comparison to prevent timing attacks
            return CryptographicOperations.FixedTimeEquals(
                Convert.FromBase64String(expectedToken),
                Convert.FromBase64String(token));
        }

        /// <summary>
        /// Creates a token with timestamp to enable token expiration
        /// </summary>
        /// <param name="data">The data to create a token for</param>
        /// <param name="expirationDays">Days until the token expires</param>
        /// <returns>Data and token separated by a delimiter</returns>
        public string GenerateTimedToken(string data, int expirationDays = 7)
        {
            string timestamp = DateTimeOffset.UtcNow.AddDays(expirationDays).ToUnixTimeSeconds().ToString();
            string dataWithTimestamp = $"{data}|{timestamp}";
            string token = GenerateToken(dataWithTimestamp);
            
            return $"{dataWithTimestamp}|{token}";
        }

        /// <summary>
        /// Validates a timed token and checks if it has expired
        /// </summary>
        /// <param name="tokenData">The complete token data (data|timestamp|token)</param>
        /// <returns>True if token is valid and not expired</returns>
        public bool ValidateTimedToken(string tokenData)
        {
            string[] parts = tokenData.Split('|');
            if (parts.Length != 3)
            {
                return false;
            }
            string data = parts[0];
            string timestamp = parts[1];
            string token = parts[2];

            // Check if token has expired
            if (!long.TryParse(timestamp, out long expirationTime))
            {
                return false;
            }

            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expirationTime)
            {
                return false;
            }
            // Validate the token
            return ValidateToken($"{data}|{timestamp}", token);
        }
    }
}
