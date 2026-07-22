using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;

namespace multiwhats_api.src.usecases.interfaces.ChatInterfaces;

public interface ICreateChatUseCase
{
    Task<ChatDetailResponse> Execute(CreateChatRequest request);
}
