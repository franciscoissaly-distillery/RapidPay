using System.Security.Cryptography;

namespace RapidPay.Api.Framework.Authentication
{
    public class KeyGenerator
    {
        public string Generate256BitKey()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var key = new byte[32];
                rng.GetBytes(key);
                return Convert.ToBase64String(key);
            }
        }
    }
}