using System.Security.Cryptography;

namespace RapidPay.Api.Auth
{
    public class KeyGenerator
    {
        public string Generate256BitKey()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] key = new byte[32];
                rng.GetBytes(key);
                return Convert.ToBase64String(key);
            }
        }
    }
}