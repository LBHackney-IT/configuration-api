using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConfigurationApi.V1.Domain;

namespace ConfigurationApi.V1.UseCase
{
    public interface IConfigurationUseCase
    {
        Task<List<ApiConfiguration>> Get(string[] types);
    }
}
