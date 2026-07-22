using multiwhats_api.src.data.dtos.Responses;

namespace multiwhats_api.src.usecases.interfaces.ContactInterfaces;

public interface IGetContactsUseCase
{
    Task<List<ContactListResponse>> ExecuteAll();
    Task<ContactDetailResponse?> ExecuteById(int id);
    Task<ContactDetailResponse?> ExecuteByPhoneNumber(string phoneNumber);
}
