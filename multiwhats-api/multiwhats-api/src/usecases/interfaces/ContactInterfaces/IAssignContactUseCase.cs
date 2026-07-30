using multiwhats_api.src.data.dtos.Responses;

namespace multiwhats_api.src.usecases.interfaces.ContactInterfaces;

public interface IAssignContactUseCase
{
    Task<ContactDetailResponse> Assign(int contactId, int clientId);
    Task<ContactDetailResponse> Unassign(int contactId);
}
