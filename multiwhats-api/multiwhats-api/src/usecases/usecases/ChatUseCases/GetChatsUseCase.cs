using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.ChatInterfaces;

namespace multiwhats_api.src.usecases.usecases.ChatUseCases;

public class GetChatsUseCase : IGetChatsUseCase
{
    private readonly IChatRepository _chatRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IOccurrenceRepository _occurrenceRepository;
    private readonly UseCaseLogger _useCaseLogger;

    public GetChatsUseCase(
        IChatRepository chatRepository,
        IMessageRepository messageRepository,
        IOccurrenceRepository occurrenceRepository,
        UseCaseLogger useCaseLogger)
    {
        _chatRepository = chatRepository;
        _messageRepository = messageRepository;
        _occurrenceRepository = occurrenceRepository;
        _useCaseLogger = useCaseLogger;
    }

    public async Task<PaginatedResponse<ChatResponse>> ExecuteAll(int page, int pageSize)
    {
        var chats = await _chatRepository.GetAllAsync(page, pageSize);
        var totalCount = await _chatRepository.GetTotalCountAsync();

        var responses = new List<ChatResponse>();
        foreach (var chat in chats)
        {
            var msgCount = await _messageRepository.GetByChatTotalCountAsync(chat.Id);
            var occCount = await _occurrenceRepository.GetByChatAsync(chat.Id);
            responses.Add(new ChatResponse
            {
                Id = chat.Id,
                Jid = chat.Jid,
                PhoneNumber = chat.PhoneNumber,
                Name = chat.Name ?? chat.Contact?.Name ?? chat.PhoneNumber,
                ContactId = chat.ContactId,
                ContactName = chat.Contact?.Name,
                ClientId = chat.ClientId,
                ClientName = chat.Client?.Name,
                LastMessageAt = chat.LastMessageAt,
                LastMessageBody = chat.LastMessageBody,
                AssignedToUserId = chat.AssignedToUserId,
                AssignedToUserName = chat.AssignedTo?.Name,
                CreatedByUserId = chat.CreatedByUserId,
                MessageCount = msgCount,
                OccurrenceCount = occCount.Count,
                CreatedAt = chat.CreatedAt,
                LastUpdate = chat.LastUpdate
            });
        }

        await _useCaseLogger.LogAsync(
            action: "GetChats",
            entityType: "Chat",
            entityId: null,
            description: $"Listed chats (page {page}, pageSize {pageSize}, total {totalCount})"
        );

        return new PaginatedResponse<ChatResponse>
        {
            Items = responses,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<ChatResponse?> ExecuteById(int id)
    {
        var chat = await _chatRepository.GetByIdAsync(id);
        if (chat == null) return null;

        var msgCount = await _messageRepository.GetByChatTotalCountAsync(chat.Id);

        await _useCaseLogger.LogAsync(
            action: "GetChat",
            entityType: "Chat",
            entityId: id,
            description: $"Retrieved chat #{id} (Jid: {chat.Jid})"
        );

        return new ChatResponse
        {
            Id = chat.Id,
            Jid = chat.Jid,
            PhoneNumber = chat.PhoneNumber,
            Name = chat.Name ?? chat.Contact?.Name ?? chat.PhoneNumber,
            ContactId = chat.ContactId,
            ContactName = chat.Contact?.Name,
            ClientId = chat.ClientId,
            ClientName = chat.Client?.Name,
            LastMessageAt = chat.LastMessageAt,
            LastMessageBody = chat.LastMessageBody,
            AssignedToUserId = chat.AssignedToUserId,
            AssignedToUserName = chat.AssignedTo?.Name,
            CreatedByUserId = chat.CreatedByUserId,
            MessageCount = msgCount,
            OccurrenceCount = chat.Occurrences?.Count ?? 0,
            CreatedAt = chat.CreatedAt,
            LastUpdate = chat.LastUpdate
        };
    }
}
