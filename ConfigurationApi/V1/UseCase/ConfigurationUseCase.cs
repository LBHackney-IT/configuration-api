using System.Collections.Generic;
using System.Threading.Tasks;
using ConfigurationApi.V1.Domain;
using ConfigurationApi.V1.Gateway;

namespace ConfigurationApi.V1.UseCase
{
    public class ConfigurationUseCase : IConfigurationUseCase
    {
        private readonly IConfigurationGateway _configurationGateway;

        public ConfigurationUseCase(IConfigurationGateway configurationGateway)
        {
            _configurationGateway = configurationGateway;
        }

        public async Task<List<ApiConfiguration>> Get(string[] types)
        {
            var listOfConfigurations = new List<ApiConfiguration>();

            foreach (string type in types)
            {
                listOfConfigurations.Add(await _configurationGateway.Get(type));
            }

            return listOfConfigurations;
        }
    }
}
