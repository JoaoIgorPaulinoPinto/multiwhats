using multiwhats_api.src.data.dtos.Responses;

namespace multiwhats_api.src.usecases.interfaces.MessageInterfaces;

public interface IMarkChatMessagesAsReadUseCase
{
    Task<List<MessageDetailResponse>> Execute(int chatId);
}
