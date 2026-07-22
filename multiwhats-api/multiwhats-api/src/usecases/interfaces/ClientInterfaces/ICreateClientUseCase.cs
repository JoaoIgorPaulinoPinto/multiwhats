using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;

namespace multiwhats_api.src.usecases.interfaces.ClientInterfaces;

public interface ICreateClientUseCase
{
    Task<ClientDetailResponse> Execute(CreateClientRequest request, int userId);
}
