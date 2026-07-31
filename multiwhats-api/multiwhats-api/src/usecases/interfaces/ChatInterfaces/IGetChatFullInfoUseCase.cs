using multiwhats_api.src.data.dtos.Responses;

namespace multiwhats_api.src.usecases.interfaces.ChatInterfaces;

public interface IGetChatFullInfoUseCase
{
    Task<ChatFullInfoResponse?> Execute(int id);
}
