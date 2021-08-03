using ConfigurationApi.V1.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConfigurationApi.V1.UseCase
{
    public interface IConfigurationUseCase
    {
        Task<List<ApiConfiguration>> Get(string[] types);
    }
}
