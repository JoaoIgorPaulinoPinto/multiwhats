using multiwhats_api.src.data.entities;

namespace multiwhats_api.src.repositories.interfaces;

public interface IOccurrenceRepository
{
    Task<List<Occurrence>> GetAllAsync();
    Task<Occurrence?> GetByIdAsync(int id);
    Task<Occurrence> AddAsync(Occurrence occurrence);
    Task<Occurrence> UpdateAsync(Occurrence occurrence);
    Task<bool> DeleteAsync(int id);
    Task<List<Occurrence>> GetByContactAsync(int contactId);
    Task<List<Occurrence>> GetByUserAsync(int userId);
}
