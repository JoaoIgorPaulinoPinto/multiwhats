using multiwhats_api.src.data.dtos.Webhook;

namespace multiwhats_api.src.usecases.interfaces.MensagemInterfaces
{
    public interface ISalvarMensagemRecebidaUseCase
    {
        public Task<bool> Execute(WhatsappMessageDto payload, int UsuarioId);
    }
}
