using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.ContactInterfaces;

namespace multiwhats_api.src.usecases.usecases.ContactUseCases;

public class AssignContactUseCase : IAssignContactUseCase
{
    private readonly IContactRepository _contactRepository;
    private readonly UseCaseLogger _useCaseLogger;

    public AssignContactUseCase(IContactRepository contactRepository, UseCaseLogger useCaseLogger)
    {
        _contactRepository = contactRepository;
        _useCaseLogger = useCaseLogger;
    }

    public async Task<ContactDetailResponse> Assign(int contactId, int clientId)
    {
        var contact = await _contactRepository.GetByIdAsync(contactId);
        if (contact == null)
            throw new KeyNotFoundException("Contato não encontrado");

        contact.AssignToClient(clientId);
        var updated = await _contactRepository.UpdateAsync(contact);

        await _useCaseLogger.LogAsync(
            action: "AssignContact",
            entityType: "Contact",
            entityId: contactId,
            description: $"Assigned contact #{contactId} (Name: {contact.Name}) to client #{clientId}"
        );

        return CreateContactUseCase.MapToDetailResponse(updated);
    }

    public async Task<ContactDetailResponse> Unassign(int contactId)
    {
        var contact = await _contactRepository.GetByIdAsync(contactId);
        if (contact == null)
            throw new KeyNotFoundException("Contato não encontrado");

        contact.UnassignFromClient();
        var updated = await _contactRepository.UpdateAsync(contact);

        await _useCaseLogger.LogAsync(
            action: "UnassignContact",
            entityType: "Contact",
            entityId: contactId,
            description: $"Unassigned contact #{contactId} (Name: {contact.Name}) from client"
        );

        return CreateContactUseCase.MapToDetailResponse(updated);
    }
}
