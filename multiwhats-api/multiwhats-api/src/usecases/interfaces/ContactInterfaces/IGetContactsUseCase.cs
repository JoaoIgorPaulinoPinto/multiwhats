using multiwhats_api.src.data.dtos.Responses;

namespace multiwhats_api.src.usecases.interfaces.ContactInterfaces;

public interface IGetContactsUseCase
{
    Task<List<ContactResponse>> ExecuteAll();
    Task<ContactResponse?> ExecuteById(int id);
    Task<ContactResponse?> ExecuteByPhoneNumber(string phoneNumber);
}
