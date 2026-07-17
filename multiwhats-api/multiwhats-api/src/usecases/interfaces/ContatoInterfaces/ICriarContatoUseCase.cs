using multiwhats_api.src.data.dtos.Requests;

namespace multiwhats_api.src.usecases.interfaces.ContatoInterfaces
{
    public interface ICriarContatoUseCase
    {
        public Task<int> Execute(CriarContatoRequest dto);
    }
}
