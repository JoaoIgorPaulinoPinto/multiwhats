using multiwhats_api.src.data.dtos.Responses;

namespace multiwhats_api.src.usecases.interfaces.ClientInterfaces;

public interface IGetClientsUseCase
{
    Task<List<ClientListResponse>> ExecuteAll();
    Task<ClientDetailResponse?> ExecuteById(int id);
}
