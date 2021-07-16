using ConfigurationApi.V1.Boundary.Response;

namespace ConfigurationApi.V1.UseCase.Interfaces
{
    public interface IGetAllUseCase
    {
        ResponseObjectList Execute();
    }
}
