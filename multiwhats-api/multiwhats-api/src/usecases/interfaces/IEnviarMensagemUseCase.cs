using multiwhats_api.src.data.dtos;

namespace multiwhats_api.src.usecases.interfaces
{
    public interface IEnviarMensagemUseCase
    {
        public Task<bool> Execute(EnviarMensagemRequest req);
    }
}
