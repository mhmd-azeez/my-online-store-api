using Microsoft.AspNetCore.Cryptography.KeyDerivation;

using System;
using System.Security.Cryptography;

namespace MyOnlineStoreAPI.Helpers
{
    public static class PasswordHelper
    {
        // https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/consumer-apis/password-hashing
        public static HashResult Hash(string password, string salt = null)
        {
            // Generate a 128-bit salt using a cryptographically strong random sequence of nonzero values
            byte[] saltBytes;

            if (salt is null)
            {
                saltBytes = new byte[128 / 8];
                using (var rngCsp = new RNGCryptoServiceProvider())
                {
                    rngCsp.GetNonZeroBytes(saltBytes);
                }

                salt = Convert.ToBase64String(saltBytes);
            }
            else
            {
                saltBytes = Convert.FromBase64String(salt);
            }

            // derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            return new HashResult
            {
                PasswordHash = hashed,
                Salt = salt
            };
        }
    }

    public class HashResult
    {
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
    }
}
