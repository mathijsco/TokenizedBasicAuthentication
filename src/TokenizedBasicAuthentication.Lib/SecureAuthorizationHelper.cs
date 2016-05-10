using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace WhiteChamber.TokenizedBasicAuthentication.Lib
{
    /// <summary>
    /// Class to read the authorization header and token, and serialized the token.
    /// </summary>
    public class SecureAuthorizationHelper
    {
        /// <summary>
        /// A random hash that will prevent mutation in the cookie.
        /// NOTE: This can be changed to any byte array for your application.
        /// </summary>
        private static readonly byte[] CookieAdditionalHash =
        {
            0x11, 0x4D, 0xED, 0x37, 0xFF, 0x01, 0x8A, 0xE6,
            0x8E, 0x80, 0xCA, 0xFF, 0x6E, 0xBB, 0x3B, 0x15
        };

        /// <summary>
        /// Deserializes the authorization header and validates if the content is specified.
        /// This will not check if the specified user name and password are valid.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <returns>
        /// The <see cref="AuthorizationHeader" /> if the header is valid, otherwise null.
        /// </returns>
        public AuthorizationHeader DeserializeHeader(string header)
        {
            if (string.IsNullOrEmpty(header))
                return null;

            // Check this is a Basic Auth header
            var parsedHeader = AuthenticationHeaderValue.Parse(header);
            if (!parsedHeader.Scheme.Equals("basic", StringComparison.OrdinalIgnoreCase))
                return null;

            // The encoding for the authorization header and decode it
            var encoding = Encoding.GetEncoding("iso-8859-1");
            var decodedHeader = encoding.GetString(Convert.FromBase64String(parsedHeader.Parameter));

            // Get the credentials with are seperated by a colon
            string[] credentials = decodedHeader.Split(new[] { ':' }, 2);
            if (credentials.Length != 2 || string.IsNullOrEmpty(credentials[0]) || string.IsNullOrEmpty(credentials[1]))
                return null;

            // Okay this are the credentials
            return new AuthorizationHeader
            {
                UserName = credentials[0],
                Password = credentials[1]
            };
        }

        /// <summary>
        /// Deserializes the authorization token and validates if it is valid.
        /// </summary>
        /// <param name="value">The serialized token.</param>
        /// <returns>The <see cref="AuthorizationToken"/>if the token is valid, otherwise null.</returns>
        public AuthorizationToken DeserializeToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            // Convert the Base64 encoded string back to bytes
            var bytes = Convert.FromBase64String(value);
            if (bytes.Length < 34) // Minimal 34 bytes long. 32 for the hash and just 2 more for other fields...
                return null;

            // Split the hash bytes from the credential text
            var hashBytes = new byte[32];
            var credBytes = new byte[bytes.Length - hashBytes.Length];
            Array.Copy(bytes, hashBytes, hashBytes.Length);
            Array.Copy(bytes, hashBytes.Length, credBytes, 0, credBytes.Length);
            
            // Check if the data is not modified by the client
            if (!GenerateHash(credBytes).SequenceEqual(hashBytes))
                return null;

            // Read the credential string again
            var cred = Encoding.UTF8.GetString(credBytes);
            var split = cred.Split('\n');
            if (split.Length != 3)
                return null;

            // Build the model
            var model = new AuthorizationToken
            {
                UserName = split[0],
                ExpirationTime = DateTime.Parse(split[1]).ToUniversalTime(),
                IsTemporary = split[2] == "1"
            };

            // Token is expired
            if (model.ExpirationTime < DateTime.UtcNow)
                return null;

            return model;
        }

        /// <summary>
        /// Serializes the token.
        /// </summary>
        /// <param name="cookie">The token.</param>
        /// <returns>The serialized token.</returns>
        public string SerializeToken(AuthorizationToken cookie)
        {
            // Get the authorization as a serialized string
            var value = Encoding.UTF8.GetBytes(string.Join("\n", new[]
            {
                cookie.UserName,
                cookie.ExpirationTime.ToString("o"),
                cookie.IsTemporary ? "1" : "0"
            }));

            // Calculate the hash of this data
            var hash = GenerateHash(value);

            // Merge the hash and credentials into a single byte array.
            var buff = new byte[hash.Length + value.Length];
            Array.Copy(hash, buff, hash.Length);
            Array.Copy(value, 0, buff, hash.Length, value.Length);

            return Convert.ToBase64String(buff);
        }

        private static byte[] GenerateHash(byte[] input)
        {
            // Merge the input and some random server side hash to make it secure. This will prevent changes on the client side.
            var buff = new byte[input.Length + CookieAdditionalHash.Length];
            Array.Copy(input, buff, input.Length);
            Array.Copy(CookieAdditionalHash, 0, buff, input.Length, CookieAdditionalHash.Length);

            using (var hasher = SHA256.Create())
            {
                return hasher.ComputeHash(buff);
            }
        }
    }
}