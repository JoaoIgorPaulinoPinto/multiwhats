using Microsoft.AspNetCore.SignalR;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.ChatInterfaces;

namespace multiwhats_api.src.usecases.usecases.ChatUseCases;

// Links an operator (user) to a chat as the person handling it ("atendimento")
// and broadcasts the change so other operators' lists stay in sync.
public class AssignChatUseCase : IAssignChatUseCase
{
    private readonly IChatRepository _chatRepository;
    private readonly UseCaseLogger _useCaseLogger;
    private readonly IHubContext<WhatsappHub> _hubContext;

    public AssignChatUseCase(
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
        var chat = await _chatRepository.AssignUserAsync(chatId, userId);
        if (chat == null)
            return null;

        await _useCaseLogger.LogAsync(
            action: "AssignChat",
            entityType: "Chat",
            entityId: chat.Id,
            description: $"Chat #{chat.Id} (Jid: {chat.Jid}) assigned to user #{userId}"
        );

        var response = new ChatAssignedResponse
        {
            Id = chat.Id,
            AssignedToUserId = chat.AssignedToUserId,
            AssignedToUserName = chat.AssignedTo?.Name
        };

        await _hubContext.Clients.All.SendAsync("ChatAssigned", response);

        return response;
    }
}
