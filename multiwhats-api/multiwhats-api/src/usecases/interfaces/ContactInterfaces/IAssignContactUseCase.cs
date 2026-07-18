using multiwhats_api.src.data.dtos.Responses;

namespace multiwhats_api.src.usecases.interfaces.ContactInterfaces;

public interface IAssignContactUseCase
{
    Task<ContactResponse> Assign(int contactId, int clientId);
    Task<ContactResponse> Unassign(int contactId);
}
