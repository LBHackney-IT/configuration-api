using System.Collections.Generic;
using System.Threading.Tasks;
using ConfigurationApi.V1.Domain;
using ConfigurationApi.V1.Gateway;
using Hackney.Core.Logging;

namespace ConfigurationApi.V1.UseCase
{
    public class ConfigurationUseCase : IConfigurationUseCase
    {
        private readonly IConfigurationGateway _configurationGateway;

        public ConfigurationUseCase(IConfigurationGateway configurationGateway)
        {
            _configurationGateway = configurationGateway;
        }

        [LogCall]
        public Task<List<ApiConfiguration>> Get(string[] types)
        {
            var listOfConfigurations = new List<ApiConfiguration>();
            var listOfTasks = new List<Task<ApiConfiguration>>();
            foreach (string type in types)
            {
                listOfTasks.Add(_configurationGateway.Get(type));
            }

            var results = Task.WhenAll(listOfTasks.ToArray());

            foreach (var result in results.Result)
            {
                if(result != null)
                    listOfConfigurations.Add(result);
            }

            return Task.FromResult(listOfConfigurations);
        }
    }
}
