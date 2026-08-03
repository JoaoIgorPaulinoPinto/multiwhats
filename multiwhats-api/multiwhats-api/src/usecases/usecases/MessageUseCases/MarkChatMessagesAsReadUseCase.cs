using Microsoft.AspNetCore.SignalR;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.MessageInterfaces;

namespace multiwhats_api.src.usecases.usecases.MessageUseCases;

// Marks incoming messages of a chat as read (viewed by the operator) and
// broadcasts the changes in real time so other open sessions stay in sync.
public class MarkChatMessagesAsReadUseCase : IMarkChatMessagesAsReadUseCase
{
    private readonly IMessageRepository _messageRepository;
    private readonly UseCaseLogger _useCaseLogger;
    private readonly IHubContext<WhatsappHub> _hubContext;

    public MarkChatMessagesAsReadUseCase(
        IMessageRepository messageRepository,
        UseCaseLogger useCaseLogger,
        IHubContext<WhatsappHub> hubContext)
    {
        _messageRepository = messageRepository;
        _useCaseLogger = useCaseLogger;
        _hubContext = hubContext;
    }

    public async Task<List<MessageDetailResponse>> Execute(int chatId)
    {
        var updated = await _messageRepository.MarkChatIncomingAsReadAsync(chatId);
        if (updated.Count == 0)
            return new List<MessageDetailResponse>();

        await _useCaseLogger.LogAsync(
            action: "MarkChatMessagesAsRead",
            entityType: "Message",
            entityId: null,
            description: $"Marked {updated.Count} incoming messages as read for chat #{chatId}"
        );

        var responses = updated.Select(GetMessagesUseCase.MapToDetailResponse).ToList();
        foreach (var response in responses)
            await _hubContext.Clients.All.SendAsync("MessageDeliveryStatusChanged", response);

        return responses;
    }
}
