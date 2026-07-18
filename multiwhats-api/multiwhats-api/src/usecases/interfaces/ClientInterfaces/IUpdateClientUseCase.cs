using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;

namespace multiwhats_api.src.usecases.interfaces.ClientInterfaces;

public interface IUpdateClientUseCase
{
    Task<ClientResponse> Execute(int id, UpdateClientRequest request);
}
