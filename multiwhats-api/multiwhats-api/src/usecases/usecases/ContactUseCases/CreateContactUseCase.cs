using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.usecases.interfaces.ContactInterfaces;

namespace multiwhats_api.src.usecases.usecases.ContactUseCases;

public class CreateContactUseCase : ICreateContactUseCase
{
    private readonly IContactRepository _contactRepository;

    public CreateContactUseCase(IContactRepository contactRepository)
    {
        _contactRepository = contactRepository;
    }

    public async Task<ContactResponse> Execute(CreateContactRequest request, int userId)
    {
        var existing = await _contactRepository.GetByJidAsync(request.Jid);
        if (existing != null)
            throw new InvalidOperationException("Já existe um contato com este JID.");

        var contact = new Contact(
            request.Jid,
            request.PhoneNumber,
            request.Name,
            request.PushName,
            userId,
            request.ClientId,
            request.GroupId
        );

        var created = await _contactRepository.AddAsync(contact);

        return MapToResponse(created);
    }

    internal static ContactResponse MapToResponse(Contact contact)
    {
        return new ContactResponse
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
