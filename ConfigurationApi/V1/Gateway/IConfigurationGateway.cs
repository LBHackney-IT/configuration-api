using System.Collections.Generic;
using System.Threading.Tasks;
using ConfigurationApi.V1.Domain;

namespace ConfigurationApi.V1.Gateway
{
    public interface IConfigurationGateway
    {
        Task<ApiConfiguration> Get(string type);
    }
}
