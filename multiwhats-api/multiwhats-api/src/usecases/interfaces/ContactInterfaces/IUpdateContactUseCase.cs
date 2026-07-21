using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;

namespace multiwhats_api.src.usecases.interfaces.ContactInterfaces;

public interface IUpdateContactUseCase
{
    Task<ContactResponse> Execute(int id, UpdateContactRequest request);
}
