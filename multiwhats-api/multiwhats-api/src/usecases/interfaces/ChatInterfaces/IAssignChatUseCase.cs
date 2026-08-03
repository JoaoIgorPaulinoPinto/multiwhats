using multiwhats_api.src.data.dtos.Responses;

namespace multiwhats_api.src.usecases.interfaces.ChatInterfaces;

public interface IAssignChatUseCase
{
    Task<ChatAssignedResponse?> Execute(int chatId, int userId);
}
