using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.usecases.interfaces.ContactInterfaces;

namespace multiwhats_api.src.usecases.usecases.ContactUseCases;

public class DeleteContactUseCase : IDeleteContactUseCase
{
    private readonly IContactRepository _contactRepository;

    public DeleteContactUseCase(IContactRepository contactRepository)
    {
        _contactRepository = contactRepository;
    }

    public async Task<bool> Execute(int contactId, int userId)
    {
        var contact = await _contactRepository.GetByIdAsync(contactId);
        if (contact == null)
            throw new KeyNotFoundException("Contato não encontrado");

        return await _contactRepository.DeleteAsync(contactId);
    }
}
