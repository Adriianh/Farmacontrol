using System.Security.Cryptography;
using System.Text;

namespace Farmacontrol.Util
{
    public class Hash
    {
        public static string Hashing(string input)
        {
            return BCrypt.Net.BCrypt.HashPassword(input);
        }

        public static bool Validate(string input, string hash)
        {
            if (string.IsNullOrEmpty(hash)) return false;

            if (!hash.StartsWith("$2") && hash.Length == 64)
            {
                byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
                string oldHash = Convert.ToHexString(bytes).ToLower();
                return oldHash == hash;
            }

            try 
            {
                return BCrypt.Net.BCrypt.Verify(input, hash);
            }
            catch (System.Exception)
            {
                return false;
            }
        }
    }
}