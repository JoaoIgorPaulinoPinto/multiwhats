using multiwhats_api.src.data.dtos.Responses;

namespace multiwhats_api.src.usecases.interfaces.ContatoInterfaces
{
    public interface IPegarContatoPorNumeroUseCase
    {
        public Task<ContatoResponse?> Execute(string numero);
    }
}
