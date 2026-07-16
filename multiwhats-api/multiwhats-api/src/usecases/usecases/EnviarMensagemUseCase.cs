using multiwhats_api.src.data.dtos;
using multiwhats_api.src.usecases.interfaces;

namespace multiwhats_api.src.usecases.usecases
{
    public class EnviarMensagemUseCase : IEnviarMensagemUseCase
    {
        public Task<bool> Execute(EnviarMensagemRequest req)
        {
            return Task.FromResult(true);
        }
    }
}
