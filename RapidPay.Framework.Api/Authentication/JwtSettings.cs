
using Microsoft.Extensions.Configuration;

namespace RapidPay.Framework.Api.Authentication
{
    public class JwtSettings
    {
        public const string ConfigSectionName = "Jwt";

        public static JwtSettings ReadFromConfiguration(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            IConfigurationSection section = configuration.GetSection(ConfigSectionName);
            if (section is null)
                throw new InvalidOperationException($"Missing configuration section '{ConfigSectionName}'");
            return section.Get<JwtSettings>()!;
        }

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

        public string SecretKey { get; }
        public string Issuer { get; }
        public string Audience { get; }
    }
}