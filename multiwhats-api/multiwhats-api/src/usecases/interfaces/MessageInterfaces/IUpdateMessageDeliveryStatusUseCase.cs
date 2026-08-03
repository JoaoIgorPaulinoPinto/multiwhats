using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.data.dtos.Webhook;

namespace multiwhats_api.src.usecases.interfaces.MessageInterfaces;

public interface IUpdateMessageDeliveryStatusUseCase
{
    Task<MessageDetailResponse?> Execute(MessageDeliveryStatusDto payload);
}
