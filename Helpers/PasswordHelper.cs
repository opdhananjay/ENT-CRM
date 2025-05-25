using System.Security.Cryptography;

namespace ENT.Helpers
{
    public class PasswordHelper
    {
        public static byte[] GenerateRandomSalt(int size = 16)
        {
            var salt = new byte[size];
            using(var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        public static (string PasswordSalt,string PasswordHash) CreatePasswordHash(string password)
        {
            var salt = GenerateRandomSalt();
            var hash = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            string saltbase64 = Convert.ToBase64String(salt);
            string hashbase64 = Convert.ToBase64String(hash.GetBytes(32));
            return (saltbase64, hashbase64);
        }

        public static bool VerifyPassword(string enteredPassword,string storeHash,string storeSalt)
        {
            byte[] saltBytes = Convert.FromBase64String(storeSalt);
            var hash = new Rfc2898DeriveBytes(enteredPassword, saltBytes, 10000, HashAlgorithmName.SHA256);
            string hashbase64 = Convert.ToBase64String(hash.GetBytes(32));
            return hashbase64 == storeHash;
        }

    }
}
