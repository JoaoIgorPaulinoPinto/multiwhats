using multiwhats_api.src.data.dtos.Responses;

namespace multiwhats_api.src.usecases.interfaces.MessageInterfaces;

public interface IGetMessagesUseCase
{
    Task<List<MessageResponse>> ExecuteAll();
    Task<MessageResponse?> ExecuteById(int id);
    Task<List<MessageResponse>> ExecuteByContact(int contactId);
    Task<List<MessageResponse>> ExecuteByPhoneNumber(string phoneNumber);
}
