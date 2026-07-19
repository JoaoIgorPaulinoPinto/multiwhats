using multiwhats_api.src.data.dtos.Responses;

namespace multiwhats_api.src.usecases.interfaces.ChatInterfaces;

public interface IGetChatsUseCase
{
    Task<PaginatedResponse<ChatResponse>> ExecuteAll(int page, int pageSize);
    Task<ChatResponse?> ExecuteById(int id);
}
