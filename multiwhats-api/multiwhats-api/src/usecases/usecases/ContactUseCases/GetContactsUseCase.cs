using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.helpers;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.ContactInterfaces;

namespace multiwhats_api.src.usecases.usecases.ContactUseCases;

public class GetContactsUseCase : IGetContactsUseCase
{
    private readonly IContactRepository _contactRepository;
    private readonly UseCaseLogger _useCaseLogger;

    public GetContactsUseCase(IContactRepository contactRepository, UseCaseLogger useCaseLogger)
    {
        _contactRepository = contactRepository;
        _useCaseLogger = useCaseLogger;
    }

    public async Task<List<ContactListResponse>> ExecuteAll()
    {
        var contacts = await _contactRepository.GetAllAsync();

        await _useCaseLogger.LogAsync(
            action: "GetContacts",
            entityType: "Contact",
            entityId: null,
            description: $"Listed all contacts (count: {contacts.Count})"
        );

        return contacts.Select(CreateContactUseCase.MapToListResponse).ToList();
    }

    public async Task<ContactDetailResponse?> ExecuteById(int id)
    {
        var contact = await _contactRepository.GetByIdAsync(id);

        await _useCaseLogger.LogAsync(
            action: "GetContact",
            entityType: "Contact",
            entityId: id,
            description: contact != null
                ? $"Retrieved contact #{id} (Name: {contact.Name}, Jid: {contact.Jid})"
                : $"Contact #{id} not found"
        );

        return contact != null ? CreateContactUseCase.MapToDetailResponse(contact) : null;
    }

    public async Task<ContactDetailResponse?> ExecuteByPhoneNumber(string phoneNumber)
    {
        var sanitized = PhoneNumberHelper.Sanitize(phoneNumber);
        var contact = await _contactRepository.GetByPhoneNumberAsync(sanitized);

        await _useCaseLogger.LogAsync(
            action: "GetContact",
            entityType: "Contact",
            entityId: contact?.Id,
            description: contact != null
                ? $"Retrieved contact by phone {sanitized} (Name: {contact.Name})"
                : $"Contact not found for phone {sanitized}"
        );

        return contact != null ? CreateContactUseCase.MapToDetailResponse(contact) : null;
    }
}
