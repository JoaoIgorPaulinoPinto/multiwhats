using multiwhats_api.src.data.dtos.Responses;

namespace multiwhats_api.src.usecases.interfaces.ContatoInterfaces
{
    public interface IPegarContatos
    {
        Task<ContatoResponse?> Execute(int userId, string numero);
        Task<ContatoResponse?> Execute(int contatoId);
        Task<List<ContatoResponse>?> Execute();
        Task<List<ContatoResponse>?> Execute(int userId, int groupId);
    }
}
