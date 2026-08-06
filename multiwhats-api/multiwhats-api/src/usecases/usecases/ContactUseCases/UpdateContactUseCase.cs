using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.ContactInterfaces;

namespace multiwhats_api.src.usecases.usecases.ContactUseCases;

public class UpdateContactUseCase : IUpdateContactUseCase
{
    private readonly IContactRepository _contactRepository;
    private readonly IChatRepository _chatRepository;
    private readonly UseCaseLogger _useCaseLogger;

    public UpdateContactUseCase(
        IContactRepository contactRepository,
        IChatRepository chatRepository,
        UseCaseLogger useCaseLogger)
    {
        _contactRepository = contactRepository;
        _chatRepository = chatRepository;
        _useCaseLogger = useCaseLogger;
    }

    public async Task<ContactDetailResponse> Execute(int id, UpdateContactRequest request)
    {
        var contact = await _contactRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Contato não encontrado.");

        contact.UpdateInfo(request.Name, request.PushName, null, request.IsBlocked);

        var updated = await _contactRepository.UpdateAsync(contact);

        // Mantém o nome do chat sincronizado com o nome salvo no contato: o
        // nome cadastrado do contato é a fonte de verdade do nome do chat.
        if (!string.IsNullOrWhiteSpace(contact.Name))
        {
            var chat = await _chatRepository.GetByJidAsync(contact.Jid);
            if (chat != null && chat.Name != contact.Name)
            {
                chat.UpdateName(contact.Name);
                await _chatRepository.UpdateAsync(chat);
            }
        }

        await _useCaseLogger.LogAsync(
            action: "UpdateContact",
            entityType: "Contact",
            entityId: updated.Id,
            description: $"Updated contact \"{updated.Name}\" (Id: {updated.Id})",
            explicitUserId: null
        );

        return CreateContactUseCase.MapToDetailResponse(updated);
    }
}
