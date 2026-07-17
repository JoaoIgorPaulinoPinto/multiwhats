using multiwhats_api.src.data.entities;

namespace multiwhats_api.src.repositories.interfaces;

public interface IOcorrenciaRepository
{
    Task<List<Ocorrencia>> GetAllAsync();
    Task<Ocorrencia?> GetByIdAsync(int id);
    Task<Ocorrencia> AddAsync(Ocorrencia ocorrencia);
    Task<Ocorrencia> UpdateAsync(Ocorrencia ocorrencia);
    Task DeleteAsync(int id);
    Task<List<Ocorrencia>> GetByStatusAsync(int statusId);
}
