using multiwhats_api.src.data.dtos;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.usecases.interfaces;

namespace multiwhats_api.src.usecases.usecases;

public class EnviarMensagemUseCase : IEnviarMensagemUseCase
{
    private readonly IMensagemRepository _mensagemRepository;

    public EnviarMensagemUseCase(IMensagemRepository mensagemRepository)
    {
        _mensagemRepository = mensagemRepository;
    }

    public async Task<bool> Execute(EnviarMensagemRequest req)
    {
        // TODO: integrar com API do WhatsApp
        return await Task.FromResult(true);
    }
}
