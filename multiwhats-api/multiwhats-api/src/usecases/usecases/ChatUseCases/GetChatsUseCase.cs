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

    public async Task<PaginatedResponse<ChatListResponse>> ExecuteAll(int page, int pageSize)
    {
        var chats = await _chatRepository.GetAllAsync(page, pageSize);
        var totalCount = await _chatRepository.GetTotalCountAsync();

        var responses = new List<ChatListResponse>();
        foreach (var chat in chats)
        {
            var msgCount = await _messageRepository.GetByChatTotalCountAsync(chat.Id);
            var occurrences = await _occurrenceRepository.GetByChatAsync(chat.Id);

            var occurrenceSummaries = occurrences.Select(o => new ChatOccurrenceSummaryResponse
            {
                Id = o.Id,
                Title = o.Title,
                Status = o.Status,
                Priority = o.Priority,
                AssignedToName = o.AssignedTo?.Name,
                MessageCount = o.Messages?.Count ?? 0,
                CreatedAt = o.CreatedAt
            }).ToList();

            responses.Add(new ChatListResponse
            {
                Id = chat.Id,
                Name = chat.Name ?? chat.Contact?.Name ?? chat.PhoneNumber ?? "Desconhecido",
                PhoneNumber = chat.PhoneNumber,
                ContactName = chat.Contact?.Name,
                ClientId = chat.ClientId,
                ClientName = chat.Client?.Name,
                LastMessageAt = chat.LastMessageAt,
                LastMessageBody = chat.LastMessageBody,
                AssignedToUserName = chat.AssignedTo?.Name,
                MessageCount = msgCount,
                Occurrences = occurrenceSummaries,
                CreatedAt = chat.CreatedAt
            });
        }

        await _useCaseLogger.LogAsync(
            action: "GetChats",
            entityType: "Chat",
            entityId: null,
            description: $"Listed chats (page {page}, pageSize {pageSize}, total {totalCount})"
        );

        return new PaginatedResponse<ChatListResponse>
        {
            Items = responses,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<ChatDetailResponse?> ExecuteById(int id)
    {
        var chat = await _chatRepository.GetByIdAsync(id);
        if (chat == null) return null;

        var msgCount = await _messageRepository.GetByChatTotalCountAsync(chat.Id);
        var occurrences = await _occurrenceRepository.GetByChatAsync(chat.Id);

        var occurrenceDetails = occurrences.Select(o => new OccurrenceDetailResponse
        {
            Id = o.Id,
            Title = o.Title,
            Description = o.Description,
            Status = o.Status,
            Priority = o.Priority,
            ChatId = o.ChatId,
            AssignedToUserId = o.AssignedToUserId,
            AssignedToName = o.AssignedTo?.Name,
            CreatedByUserId = o.CreatedByUserId,
            CreatedByName = o.CreatedBy?.Name,
            MessageCount = o.Messages?.Count ?? 0,
            CreatedAt = o.CreatedAt,
            LastUpdate = o.LastUpdate
        }).ToList();

        await _useCaseLogger.LogAsync(
            action: "GetChat",
            entityType: "Chat",
            entityId: id,
            description: $"Retrieved chat #{id} (Jid: {chat.Jid})"
        );

        return new ChatDetailResponse
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
            Occurrences = occurrenceDetails,
            CreatedByUserId = chat.CreatedByUserId,
            MessageCount = msgCount,
            OccurrenceCount = occurrences.Count,
            CreatedAt = chat.CreatedAt,
            LastUpdate = chat.LastUpdate
        };
    }
}
