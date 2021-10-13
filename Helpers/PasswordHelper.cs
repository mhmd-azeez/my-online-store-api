using Microsoft.AspNetCore.Cryptography.KeyDerivation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MyOnlineStoreAPI.Helpers
{
    public static class PasswordHelper
    {
        public static PasswordHashResult HashPassword(string plaintext, string salt = null)
        {
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

            string hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: plaintext,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            return new PasswordHashResult
            {
                PasswordSalt = salt,
                PasswordHash = hash
            };
        }
    }

    public class PasswordHashResult
    {
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
    }
}
