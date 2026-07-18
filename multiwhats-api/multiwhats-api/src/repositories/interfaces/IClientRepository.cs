using multiwhats_api.src.data.entities;

namespace multiwhats_api.src.repositories.interfaces;

public interface IClientRepository
{
    Task<List<Client>> GetAllAsync();
    Task<Client?> GetByIdAsync(int id);
    Task<Client> AddAsync(Client client);
    Task<Client> UpdateAsync(Client client);
    Task<bool> DeleteAsync(int id);
    Task<List<Contact>> GetContactsAsync(int clientId);
    Task<List<ClientTask>> GetTasksAsync(int clientId);
    Task<int> GetContactCountAsync(int clientId);
}
