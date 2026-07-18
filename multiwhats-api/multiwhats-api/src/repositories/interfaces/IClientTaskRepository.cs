using multiwhats_api.src.data.entities;

namespace multiwhats_api.src.repositories.interfaces;

public interface IClientTaskRepository
{
    Task<List<ClientTask>> GetAllAsync();
    Task<ClientTask?> GetByIdAsync(int id);
    Task<ClientTask> AddAsync(ClientTask task);
    Task<ClientTask> UpdateAsync(ClientTask task);
    Task<bool> DeleteAsync(int id);
    Task<List<ClientTask>> GetByClientAsync(int clientId);
    Task<List<ClientTask>> GetByUserAsync(int userId);
}
