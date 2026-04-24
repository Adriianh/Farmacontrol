using System.Security.Cryptography;
using System.Text;

namespace Farmacontrol.Util
{
    public class Hash
    {
        public static string Hashing(string input)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes).ToLower();
        }
    }
}