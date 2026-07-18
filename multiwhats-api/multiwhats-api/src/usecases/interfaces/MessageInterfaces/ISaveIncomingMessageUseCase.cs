using multiwhats_api.src.data.dtos.Webhook;

namespace multiwhats_api.src.usecases.interfaces.MessageInterfaces;

public interface ISaveIncomingMessageUseCase
{
    Task<bool> Execute(WhatsAppWebhookDto payload);
}
