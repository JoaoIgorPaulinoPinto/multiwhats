using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.usecases.interfaces.ContactInterfaces;

namespace multiwhats_api.src.usecases.usecases.ContactUseCases;

public class GetContactsUseCase : IGetContactsUseCase
{
    private readonly IContactRepository _contactRepository;

    public GetContactsUseCase(IContactRepository contactRepository)
    {
        _contactRepository = contactRepository;
    }

    public async Task<List<ContactResponse>> ExecuteAll()
    {
        var contacts = await _contactRepository.GetAllAsync();
        return contacts.Select(CreateContactUseCase.MapToResponse).ToList();
    }

    public async Task<ContactResponse?> ExecuteById(int id)
    {
        var contact = await _contactRepository.GetByIdAsync(id);
        return contact != null ? CreateContactUseCase.MapToResponse(contact) : null;
    }

    public async Task<ContactResponse?> ExecuteByPhoneNumber(string phoneNumber)
    {
        var contact = await _contactRepository.GetByPhoneNumberAsync(phoneNumber);
        return contact != null ? CreateContactUseCase.MapToResponse(contact) : null;
    }
}
