using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.usecases.interfaces.ContactInterfaces;

namespace multiwhats_api.src.usecases.usecases.ContactUseCases;

public class AssignContactUseCase : IAssignContactUseCase
{
    private readonly IContactRepository _contactRepository;

    public AssignContactUseCase(IContactRepository contactRepository)
    {
        _contactRepository = contactRepository;
    }

    public async Task<ContactResponse> Assign(int contactId, int clientId)
    {
        var contact = await _contactRepository.GetByIdAsync(contactId);
        if (contact == null)
            throw new KeyNotFoundException("Contato não encontrado");

        contact.AssignToClient(clientId);
        var updated = await _contactRepository.UpdateAsync(contact);

        return CreateContactUseCase.MapToResponse(updated);
    }

    public async Task<ContactResponse> Unassign(int contactId)
    {
        var contact = await _contactRepository.GetByIdAsync(contactId);
        if (contact == null)
            throw new KeyNotFoundException("Contato não encontrado");

        contact.UnassignFromClient();
        var updated = await _contactRepository.UpdateAsync(contact);

        return CreateContactUseCase.MapToResponse(updated);
    }
}
