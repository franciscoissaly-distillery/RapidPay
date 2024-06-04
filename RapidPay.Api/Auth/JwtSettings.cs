namespace RapidPay.Api.Auth
{
    public class JwtSettings
    {

        public JwtSettings(string secretKey, string issuer, string audience)
        {
            if (string.IsNullOrWhiteSpace(secretKey))
                throw new ArgumentException($"'{nameof(secretKey)}' cannot be null or whitespace.", nameof(secretKey));

            if (string.IsNullOrWhiteSpace(issuer))
                throw new ArgumentException($"'{nameof(issuer)}' cannot be null or whitespace.", nameof(issuer));

            if (string.IsNullOrWhiteSpace(audience))
                throw new ArgumentException($"'{nameof(audience)}' cannot be null or whitespace.", nameof(audience));

            SecretKey = secretKey;
            Issuer = issuer;
            Audience = audience;
        }
        //public string SecretKey { get; init; }
        //public string Issuer { get; init; }
        //public string Audience { get; init; }
        public string SecretKey { get; }
        public string Issuer { get; }
        public string Audience { get; }
    }
}