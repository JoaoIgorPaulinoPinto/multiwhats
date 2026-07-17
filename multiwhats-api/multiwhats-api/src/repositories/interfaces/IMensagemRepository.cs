using multiwhats_api.src.data.entities;

namespace multiwhats_api.src.repositories.interfaces;

public interface IMensagemRepository
{
    Task<List<Mensagem>> PegarTodosAsync();
    Task<Mensagem?> PegarPorIdAsync(int id);
    Task<bool> AdicionarAsync(Mensagem mensagem);
    Task DeleteAsync(int id);
    Task<List<Mensagem>> GetByContatoAsync(int contatoId);
}
