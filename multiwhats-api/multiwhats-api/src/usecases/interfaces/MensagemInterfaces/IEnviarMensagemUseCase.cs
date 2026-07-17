using multiwhats_api.src.data.dtos.Requests;

namespace multiwhats_api.src.usecases.interfaces.MensagemInterfaces
{
    public interface IEnviarMensagemUseCase
    {
        public Task<bool> Execute(EnviarMensagemRequest req);
    }
}
