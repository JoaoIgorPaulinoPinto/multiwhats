using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using multiwhats_api.src.data.db;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.data.enums;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.ChatInterfaces;

namespace multiwhats_api.src.usecases.usecases.ChatUseCases;

public class GetChatFullInfoUseCase : IGetChatFullInfoUseCase
{
    private readonly AppDbContext _context;
    private readonly IChatRepository _chatRepository;
    private readonly UseCaseLogger _useCaseLogger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetChatFullInfoUseCase(AppDbContext context, IChatRepository chatRepository, UseCaseLogger useCaseLogger, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _chatRepository = chatRepository;
        _useCaseLogger = useCaseLogger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ChatFullInfoResponse?> Execute(int id)
    {
        var chat = await _context.Chats
            .AsNoTracking()
            .Include(c => c.Contact)
                .ThenInclude(c => c.Client)
            .Include(c => c.Client)
            .Include(c => c.AssignedTo)
            .Include(c => c.CreatedBy)
            .Include(c => c.LastMessage)
            .Include(c => c.Occurrences)
                .ThenInclude(o => o.AssignedTo)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (chat == null) return null;

        var msgCount = await _chatRepository.GetMessageCountAsync(id);

        var typeCounts = await _context.Messages
            .Where(m => m.ChatId == id)
            .GroupBy(m => m.Type)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Type, g => g.Count);

        var directionCounts = await _context.Messages
            .Where(m => m.ChatId == id)
            .GroupBy(m => m.Direction)
            .Select(g => new { Direction = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Direction, g => g.Count);

        var mediaSentCount = await _context.Messages
            .CountAsync(m => m.ChatId == id && m.Direction == MessageDirection.Outgoing
                && (m.Type == MessageType.Image
                    || m.Type == MessageType.Audio
                    || m.Type == MessageType.Video
                    || m.Type == MessageType.Document
                    || m.Type == MessageType.Sticker));

        var mediaCount = typeCounts.GetValueOrDefault(MessageType.Image)
            + typeCounts.GetValueOrDefault(MessageType.Video)
            + typeCounts.GetValueOrDefault(MessageType.Audio)
            + typeCounts.GetValueOrDefault(MessageType.Document)
            + typeCounts.GetValueOrDefault(MessageType.Sticker);

        var lastOccurrence = chat.Occurrences.OrderByDescending(o => o.LastUpdate).FirstOrDefault();
        var currentUserId = GetCurrentUserId();

        var occurrenceSummaries = chat.Occurrences.Select(o => new ChatOccurrenceSummaryResponse
        {
            Id = o.Id,
            Title = o.Title,
            Status = o.Status,
            Priority = o.Priority,
            AssignedToName = o.AssignedTo?.Name,
            MessageCount = o.Messages?.Count ?? 0,
            CreatedAt = o.CreatedAt,
            byMe = o.CreatedByUserId == currentUserId
        }).ToList();

        await _useCaseLogger.LogAsync(
            action: "GetChatFullInfo",
            entityType: "Chat",
            entityId: id,
            description: $"Retrieved full info for chat #{id} (Jid: {chat.Jid})"
        );

        return new ChatFullInfoResponse
        {
            Id = chat.Id,
            Jid = chat.Jid,
            PhoneNumber = chat.PhoneNumber,
            Name = chat.Name ?? chat.Contact?.Name ?? chat.PhoneNumber,
            ContactId = chat.ContactId,
            ContactName = chat.Contact?.Name,
            ContactPushName = chat.Contact?.PushName,
            ContactProfilePicUrl = chat.Contact?.ProfilePicUrl,
            ContactIsBlocked = chat.Contact?.IsBlocked ?? false,
            ContactIsGroup = chat.Contact?.IsGroup ?? false,
            ClientId = chat.ClientId ?? chat.Contact?.ClientId,
            ClientName = chat.Client?.Name ?? chat.Contact?.Client?.Name,
            ClientMainPhoneNumber = chat.Client?.MainPhoneNumber ?? chat.Contact?.Client?.MainPhoneNumber,
            AssignedToUserId = chat.AssignedToUserId,
            AssignedToUserName = chat.AssignedTo?.Name,
            CreatedByUserId = chat.CreatedByUserId,
            CreatedByName = chat.CreatedBy?.Name,
            LastMessageAt = chat.LastMessageAt,
            LastMessage = chat.LastMessage != null
                ? new LastMessageResponse
                {
                    Type = chat.LastMessage.Type,
                    Body = chat.LastMessage.Body,
                    Direction = chat.LastMessage.Direction,
                    DeliveryStatus = chat.LastMessage.DeliveryStatus
                }
                : null,
            MessageCount = msgCount,
            OutgoingMessageCount = directionCounts.GetValueOrDefault(MessageDirection.Outgoing),
            IncomingMessageCount = directionCounts.GetValueOrDefault(MessageDirection.Incoming),
            ImageCount = typeCounts.GetValueOrDefault(MessageType.Image),
            VideoCount = typeCounts.GetValueOrDefault(MessageType.Video),
            AudioCount = typeCounts.GetValueOrDefault(MessageType.Audio),
            DocumentCount = typeCounts.GetValueOrDefault(MessageType.Document),
            TextCount = typeCounts.GetValueOrDefault(MessageType.Text),
            StickerCount = typeCounts.GetValueOrDefault(MessageType.Sticker),
            MediaCount = mediaCount,
            MediaSentCount = mediaSentCount,
            DaysActive = Math.Max(1, (int)Math.Ceiling((DateTime.UtcNow - chat.CreatedAt).TotalDays)),
            TimeSinceLastOccurrenceSeconds = lastOccurrence != null
                ? (long?)(DateTime.UtcNow - lastOccurrence.LastUpdate).TotalSeconds
                : null,
            Occurrences = occurrenceSummaries,
            OccurrenceCount = occurrenceSummaries.Count,
            CreatedAt = chat.CreatedAt,
            LastUpdate = chat.LastUpdate
        };
    }

    private int? GetCurrentUserId()
    {
        var value = _httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return value != null && int.TryParse(value, out var id) ? id : null;
    }
}
