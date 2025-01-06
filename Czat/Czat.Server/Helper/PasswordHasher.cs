using System;
using System.Security.Cryptography;
using System.Text;

namespace Czat.Server.Helper
{
    public class PasswordHasher
    {
        private const int SaltSize = 16; // 16 bytes for salt
        private const int Iterations = 10000; // Number of iterations for PBKDF2
    
        public byte[] GenerateSalt()
        {
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[SaltSize]);
            return salt;
        }
    
        public byte[] HashPassword(string password, byte[] salt)
        {
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations);
            return pbkdf2.GetBytes(20); // 20 bytes hash size (SHA-1)
        }
    }
    
}

