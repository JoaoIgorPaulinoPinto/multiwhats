using multiwhats_api.src.data.dtos.Responses;

namespace multiwhats_api.src.usecases.interfaces.ContatoInterfaces
{
    public interface IPegarContatos
    {
        //buscar contato por numero
        Task<ContatoResponse?> Execute(string numero, int userId);
        //buscar contato por ID
        Task<ContatoResponse?> Execute(int contatoId, int userId);
        //buscar todos os contatos
        Task<List<ContatoResponse>?> Execute( int userId);
    }
}
