using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.helpers;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.ContactInterfaces;

namespace multiwhats_api.src.usecases.usecases.ContactUseCases;

// Creates a contact and auto-links to an existing chat with the same JID.
public class CreateContactUseCase : ICreateContactUseCase
{
    private readonly IContactRepository _contactRepository;
    private readonly IChatRepository _chatRepository;
    private readonly UseCaseLogger _useCaseLogger;

    public CreateContactUseCase(
        IContactRepository contactRepository,
        IChatRepository chatRepository,
        UseCaseLogger useCaseLogger)
    {
        _contactRepository = contactRepository;
        _chatRepository = chatRepository;
        _useCaseLogger = useCaseLogger;
    }


    public async Task<ContactDetailResponse> Execute(CreateContactRequest request, int userId)
    {
        var existing = await _contactRepository.GetByJidAsync(request.Jid);
        if (existing != null)
            throw new InvalidOperationException("Já existe um contato com este JID.");

        var contact = new Contact(
            request.Jid,
            PhoneNumberHelper.Sanitize(request.PhoneNumber),
            request.Name,
            request.PushName,
            userId,
            request.ClientId,
            request.GroupId
        );

        var created = await _contactRepository.AddAsync(contact);

        var existingChat = await _chatRepository.GetByJidAsync(request.Jid);
        if (existingChat != null)
        {
            existingChat.LinkToContact(created.Id, created.ClientId);
            if (!string.IsNullOrWhiteSpace(created.Name))
                existingChat.UpdateName(created.Name);
            // Se o chat já tinha foto (capturada pelo messageria antes do
            // contato ser salvo) e o contato ainda não tem, repassa a URL.
            if (string.IsNullOrWhiteSpace(created.ProfilePicUrl)
                && !string.IsNullOrWhiteSpace(existingChat.ProfilePicUrl))
            {
                created.UpdateInfo(name: null, pushName: null, profilePicUrl: existingChat.ProfilePicUrl, isBlocked: null);
                await _contactRepository.UpdateAsync(created);
            }
            await _chatRepository.UpdateAsync(existingChat);
        }

        await _useCaseLogger.LogAsync(
            action: "CreateContact",
            entityType: "Contact",
            entityId: created.Id,
            description: $"Created contact \"{created.Name}\" (Jid: {created.Jid}, ClientId: {created.ClientId})",
            explicitUserId: userId
        );

        return MapToDetailResponse(created);
    }

    internal static ContactListResponse MapToListResponse(Contact contact)
    {
        return new ContactListResponse
        {
            Id = contact.Id,
            Name = contact.Name,
            PhoneNumber = contact.PhoneNumber,
            PushName = contact.PushName,
            ProfilePicUrl = contact.ProfilePicUrl,
            IsBlocked = contact.IsBlocked,
            IsGroup = contact.IsGroup,
            ClientId = contact.ClientId,
            ClientName = contact.Client?.Name,
            CreatedAt = contact.CreatedAt
        };
    }

    internal static ContactDetailResponse MapToDetailResponse(Contact contact)
    {
        return new ContactDetailResponse
        {
            Id = contact.Id,
            Jid = contact.Jid,
            PhoneNumber = contact.PhoneNumber,
            Name = contact.Name,
            PushName = contact.PushName,
            ProfilePicUrl = contact.ProfilePicUrl,
            IsBlocked = contact.IsBlocked,
            IsGroup = contact.IsGroup,
            LastMessageAt = contact.LastMessageAt,
            ClientId = contact.ClientId,
            ClientName = contact.Client?.Name,
            GroupId = contact.GroupId,
            GroupName = contact.Group?.Name,
            CreatedByUserId = contact.CreatedByUserId,
            CreatedAt = contact.CreatedAt,
            LastUpdate = contact.LastUpdate
        };
    }
}
