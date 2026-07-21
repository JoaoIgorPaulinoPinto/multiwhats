using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.ContactInterfaces;

namespace multiwhats_api.src.usecases.usecases.ContactUseCases;

public class DeleteContactUseCase : IDeleteContactUseCase
{
    private readonly IContactRepository _contactRepository;
    private readonly UseCaseLogger _useCaseLogger;

    public DeleteContactUseCase(IContactRepository contactRepository, UseCaseLogger useCaseLogger)
    {
        _contactRepository = contactRepository;
        _useCaseLogger = useCaseLogger;
    }

    public async Task<bool> Execute(int contactId, int userId)
    {
        var contact = await _contactRepository.GetByIdAsync(contactId);
        if (contact == null)
            throw new KeyNotFoundException("Contato não encontrado");

        var result = await _contactRepository.DeleteAsync(contactId);

        await _useCaseLogger.LogAsync(
            action: "DeleteContact",
            entityType: "Contact",
            entityId: contactId,
            description: $"Deleted contact #{contactId} (Name: {contact.Name}, Jid: {contact.Jid})",
            explicitUserId: userId
        );

        return result;
    }
}
