using Microsoft.Extensions.Configuration;
using System.Collections.ObjectModel;

namespace RapidPay.Api.Framework.ApiClient
{
    public class ExternalApisSettings : Collection<ExternalApiSettings>
    {
        public const string ConfigSectionName = "ExternalApis";

        public static ExternalApisSettings ReadFromConfiguration(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            IConfigurationSection section = configuration.GetSection(ConfigSectionName);
            if (section is null)
                throw new InvalidOperationException($"Missing configuration section '{ConfigSectionName}'");

            return section.Get<ExternalApisSettings>()!;
        }

        public ExternalApiSettings? GetByName(string apiName)
        {
            if (string.IsNullOrWhiteSpace(apiName))
                return null;

            return this.FirstOrDefault(x => apiName.Equals(x.Name, StringComparison.OrdinalIgnoreCase));
        }
    }
}
