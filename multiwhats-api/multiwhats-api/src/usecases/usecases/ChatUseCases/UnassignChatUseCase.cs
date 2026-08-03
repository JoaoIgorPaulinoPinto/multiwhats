using Microsoft.AspNetCore.SignalR;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.ChatInterfaces;

namespace multiwhats_api.src.usecases.usecases.ChatUseCases;

// Unlinks the operator (user) from a chat ("finalizar atendimento"),
// keeping the chat and its occurrences intact,
// and broadcasts the change so other operators' lists stay in sync.
public class UnassignChatUseCase : IUnassignChatUseCase
{
    private readonly IChatRepository _chatRepository;
    private readonly UseCaseLogger _useCaseLogger;
    private readonly IHubContext<WhatsappHub> _hubContext;

    public UnassignChatUseCase(
        IChatRepository chatRepository,
        UseCaseLogger useCaseLogger,
        IHubContext<WhatsappHub> hubContext)
    {
        _chatRepository = chatRepository;
        _useCaseLogger = useCaseLogger;
        _hubContext = hubContext;
    }

    public async Task<ChatAssignedResponse?> Execute(int chatId, int userId)
    {
        var chat = await _chatRepository.UnAssignUserAsync(chatId, userId);
        if (chat == null)
            return null;

        await _useCaseLogger.LogAsync(
            action: "UnassignChat",
            entityType: "Chat",
            entityId: chat.Id,
            description: $"Chat #{chat.Id} (Jid: {chat.Jid}) unassigned from user #{userId}"
        );

        var response = new ChatAssignedResponse
        {
            Id = chat.Id,
            AssignedToUserId = null,
            AssignedToUserName = ""
        };

        await _hubContext.Clients.All.SendAsync("ChatUnassigned", response);

        return response;
    }
}
