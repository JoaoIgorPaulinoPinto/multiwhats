using multiwhats_api.src.data.entities;

namespace multiwhats_api.src.repositories.interfaces;

public interface IStatusRepository
{
    Task<List<Status>> GetAllAsync();
    Task<Status?> GetByIdAsync(int id);
    Task<Status> AddAsync(Status status);
    Task<Status> UpdateAsync(Status status);
    Task DeleteAsync(int id);
}
