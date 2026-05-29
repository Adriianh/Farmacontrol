namespace Farmacontrol.Core.Util
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