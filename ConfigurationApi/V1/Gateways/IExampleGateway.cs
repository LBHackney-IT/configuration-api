using System.Collections.Generic;
using ConfigurationApi.V1.Domain;

namespace ConfigurationApi.V1.Gateways
{
    public interface IExampleGateway
    {
        Entity GetEntityById(int id);

        List<Entity> GetAll();
    }
}
