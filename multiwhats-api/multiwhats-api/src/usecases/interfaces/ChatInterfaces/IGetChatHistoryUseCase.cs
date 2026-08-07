using multiwhats_api.src.data.dtos.Responses;

namespace multiwhats_api.src.usecases.interfaces.ChatInterfaces;

public interface IGetChatHistoryUseCase
{
    Task<ChatHistoryResponse?> Execute(int chatId);
}
