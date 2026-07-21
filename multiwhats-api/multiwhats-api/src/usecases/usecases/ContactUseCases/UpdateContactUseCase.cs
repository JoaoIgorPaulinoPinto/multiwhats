using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.ContactInterfaces;

namespace multiwhats_api.src.usecases.usecases.ContactUseCases;

public class UpdateContactUseCase : IUpdateContactUseCase
{
    private readonly IContactRepository _contactRepository;
    private readonly UseCaseLogger _useCaseLogger;

    public UpdateContactUseCase(
        IContactRepository contactRepository,
        UseCaseLogger useCaseLogger)
    {
        _contactRepository = contactRepository;
        _useCaseLogger = useCaseLogger;
    }

    public async Task<ContactResponse> Execute(int id, UpdateContactRequest request)
    {
        var contact = await _contactRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Contato não encontrado.");

        contact.UpdateInfo(request.Name, request.PushName, null, request.IsBlocked);

        var updated = await _contactRepository.UpdateAsync(contact);

        await _useCaseLogger.LogAsync(
            action: "UpdateContact",
            entityType: "Contact",
            entityId: updated.Id,
            description: $"Updated contact \"{updated.Name}\" (Id: {updated.Id})",
            explicitUserId: null
        );

        return CreateContactUseCase.MapToResponse(updated);
    }
}
