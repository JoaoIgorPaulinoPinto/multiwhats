using multiwhats_api.src.data.dtos.Requests;

namespace multiwhats_api.src.usecases.interfaces.MessageInterfaces;

public interface ISendMessageUseCase
{
    Task<bool> Execute(SendMessageRequest request, int userId);
}
