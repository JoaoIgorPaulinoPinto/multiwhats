using multiwhats_api.src.data.entities;

namespace multiwhats_api.src.repositories.interfaces;

public interface IMensagemRepository
{
    Task<List<Mensagem>> GetAllAsync();
    Task<Mensagem?> GetByIdAsync(int id);
    Task<Mensagem> AddAsync(Mensagem mensagem);
    Task DeleteAsync(int id);
    Task<List<Mensagem>> GetByContatoAsync(int contatoId);
}
