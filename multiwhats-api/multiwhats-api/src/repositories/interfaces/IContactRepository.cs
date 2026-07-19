using multiwhats_api.src.data.entities;

namespace multiwhats_api.src.repositories.interfaces;

public interface IContactRepository
{
    Task<List<Contact>> GetAllAsync();
    Task<Contact?> GetByIdAsync(int id);
    Task<Contact?> GetByJidAsync(string jid);
    Task<Contact?> GetByPhoneNumberAsync(string phoneNumber);
    Task<Contact> AddAsync(Contact contact);
    Task<Contact> UpdateAsync(Contact contact);
    Task<bool> DeleteAsync(int id);
    Task<List<Contact>> GetByClientAsync(int clientId);
    Task<List<Contact>> GetByGroupAsync(int groupId);
}
