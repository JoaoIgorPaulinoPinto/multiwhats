using multiwhats_api.src.data.entities;

namespace multiwhats_api.src.repositories.interfaces;

public interface IGrupoRepository
{
    Task<List<Grupo>> GetAllAsync();
    Task<Grupo?> GetByIdAsync(int id);
    Task<Grupo> AddAsync(Grupo grupo);
    Task<Grupo> UpdateAsync(Grupo grupo);
    Task DeleteAsync(int id);
    Task<List<Contato>> GetMembrosAsync(int grupoId);
}
