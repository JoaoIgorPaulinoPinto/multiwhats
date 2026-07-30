using multiwhats_api.src.data.dtos.Responses;

namespace multiwhats_api.src.usecases.interfaces.MessageInterfaces;

public interface IGetMessagesUseCase
{
    Task<List<MessageSummaryResponse>> ExecuteAll();
    Task<MessageDetailResponse?> ExecuteById(int id);
    Task<PaginatedResponse<MessageSummaryResponse>> ExecuteByChat(int chatId, int page, int pageSize);
    Task<List<MessageSummaryResponse>> ExecuteByPhoneNumber(string phoneNumber);
}
