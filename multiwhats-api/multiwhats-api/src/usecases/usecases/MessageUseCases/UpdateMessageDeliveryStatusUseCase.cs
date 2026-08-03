using Microsoft.AspNetCore.SignalR;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.data.dtos.Webhook;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.MessageInterfaces;

namespace multiwhats_api.src.usecases.usecases.MessageUseCases;

// Updates the delivery status of a message from an ack/read receipt reported by
// the WhatsApp bridge (Node) and broadcasts the change in real time.
public class UpdateMessageDeliveryStatusUseCase : IUpdateMessageDeliveryStatusUseCase
{
    private readonly IMessageRepository _messageRepository;
    private readonly UseCaseLogger _useCaseLogger;
    private readonly IHubContext<WhatsappHub> _hubContext;

    public UpdateMessageDeliveryStatusUseCase(
        IMessageRepository messageRepository,
        UseCaseLogger useCaseLogger,
        IHubContext<WhatsappHub> hubContext)
    {
        _messageRepository = messageRepository;
        _useCaseLogger = useCaseLogger;
        _hubContext = hubContext;
    }

    public async Task<MessageDetailResponse?> Execute(MessageDeliveryStatusDto payload)
    {
        if (string.IsNullOrWhiteSpace(payload.MessageId))
            return null;

        var message = await _messageRepository.UpdateDeliveryStatusAsync(
            payload.MessageId, payload.DeliveryStatus);

        if (message == null)
            return null;

        await _useCaseLogger.LogAsync(
            action: "UpdateMessageDeliveryStatus",
            entityType: "Message",
            entityId: message.Id,
            description: $"Updated delivery status of message {payload.MessageId} to {payload.DeliveryStatus}"
        );

        var response = GetMessagesUseCase.MapToDetailResponse(message);
        await _hubContext.Clients.All.SendAsync("MessageDeliveryStatusChanged", response);

        return response;
    }
}
