using multiwhats_api.src.data.entities;

namespace multiwhats_api.src.repositories.interfaces;

public interface IContatoRepository
{
    Task<List<Contato>> GetAllAsync();
    Task<Contato?> GetByIdAsync(int id);
    Task<Contato?> GetByNumberAsync(string number);
    Task<Contato> AddAsync(Contato contato);
    Task<Contato> UpdateAsync(Contato contato);
    Task DeleteAsync(int id);
    Task<List<Contato>> GetByGrupoAsync(int grupoId);
    Task<List<Mensagem>> GetMensagensAsync(int contatoId);
}
