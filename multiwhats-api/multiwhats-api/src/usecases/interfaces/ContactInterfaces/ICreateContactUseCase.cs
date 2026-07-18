using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;

namespace multiwhats_api.src.usecases.interfaces.ContactInterfaces;

public interface ICreateContactUseCase
{
    Task<ContactResponse> Execute(CreateContactRequest request, int userId);
}
