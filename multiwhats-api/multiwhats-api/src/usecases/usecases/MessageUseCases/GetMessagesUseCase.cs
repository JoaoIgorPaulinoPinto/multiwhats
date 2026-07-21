using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.helpers;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.MessageInterfaces;

namespace multiwhats_api.src.usecases.usecases.MessageUseCases;

public class GetMessagesUseCase : IGetMessagesUseCase
{
    private readonly IMessageRepository _messageRepository;
    private readonly UseCaseLogger _useCaseLogger;

    public GetMessagesUseCase(IMessageRepository messageRepository, UseCaseLogger useCaseLogger)
    {
        _messageRepository = messageRepository;
        _useCaseLogger = useCaseLogger;
    }

    public async Task<List<MessageResponse>> ExecuteAll()
    {
        var messages = await _messageRepository.GetAllAsync();

        await _useCaseLogger.LogAsync(
            action: "GetMessages",
            entityType: "Message",
            entityId: null,
            description: $"Listed all messages (count: {messages.Count})"
        );

        return messages.Select(MapToResponse).ToList();
    }

    public async Task<MessageResponse?> ExecuteById(int id)
    {
        var message = await _messageRepository.GetByIdAsync(id);

        await _useCaseLogger.LogAsync(
            action: "GetMessage",
            entityType: "Message",
            entityId: id,
            description: message != null
                ? $"Retrieved message #{id} (from: {message.FromJid})"
                : $"Message #{id} not found"
        );

        return message != null ? MapToResponse(message) : null;
    }

    public async Task<PaginatedResponse<MessageResponse>> ExecuteByChat(int chatId, int page, int pageSize)
    {
        var messages = await _messageRepository.GetByChatAsync(chatId, page, pageSize);
        var totalCount = await _messageRepository.GetByChatTotalCountAsync(chatId);

        await _useCaseLogger.LogAsync(
            action: "GetMessages",
            entityType: "Message",
            entityId: null,
            description: $"Listed messages for chat #{chatId} (page {page}, pageSize {pageSize}, total {totalCount})"
        );

        return new PaginatedResponse<MessageResponse>
        {
            Items = messages.Select(MapToResponse).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<List<MessageResponse>> ExecuteByPhoneNumber(string phoneNumber)
    {
        var sanitized = PhoneNumberHelper.Sanitize(phoneNumber);
        var messages = await _messageRepository.GetByPhoneNumberAsync(sanitized);

        await _useCaseLogger.LogAsync(
            action: "GetMessages",
            entityType: "Message",
            entityId: null,
            description: $"Listed messages for phone {sanitized} (count: {messages.Count})"
        );

        return messages.Select(MapToResponse).ToList();
    }

    internal static MessageResponse MapToResponse(Message message)
    {
        return new MessageResponse
        {
            Id = message.Id,
            MessageId = message.MessageId,
            FromJid = message.FromJid,
            ToJid = message.ToJid,
            PhoneNumber = message.PhoneNumber,
            Body = message.Body,
            Direction = message.Direction,
            Type = message.Type,
            Timestamp = message.Timestamp,
            SentAt = message.SentAt,
            NotifyName = message.NotifyName,
            HasMedia = message.HasMedia,
            MediaUrl = message.MediaUrl,
            MediaMimeType = message.MediaMimeType,
            MediaFilename = message.MediaFilename,
            MediaSize = message.MediaSize,
            MediaCaption = message.MediaCaption,
            DeliveryStatus = message.DeliveryStatus,
            IsForwarded = message.IsForwarded,
            ChatId = message.ChatId,
            UserId = message.UserId,
            OccurrenceId = message.OccurrenceId,
            ReplyToId = message.ReplyToId,
            CreatedAt = message.CreatedAt,
        };
    }
}
