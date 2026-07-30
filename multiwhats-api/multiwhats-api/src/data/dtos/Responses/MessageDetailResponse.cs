using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.dtos.Responses;

public record MessageDetailResponse
{
    public int Id { get; init; }
    public string? MessageId { get; init; }
    public string FromJid { get; init; } = null!;
    public string? ToJid { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Body { get; init; }
    public MessageDirection Direction { get; init; }
    public MessageType Type { get; init; }
    public long Timestamp { get; init; }
    public DateTime SentAt { get; init; }
    public string? NotifyName { get; init; }
    public bool HasMedia { get; init; }
    public string? MediaUrl { get; init; }
    public string? MediaMimeType { get; init; }
    public string? MediaFilename { get; init; }
    public long? MediaSize { get; init; }
    public string? MediaCaption { get; init; }
    public DeliveryStatus DeliveryStatus { get; init; }
    public bool IsForwarded { get; init; }
    public int ChatId { get; init; }
    public int? UserId { get; init; }
    public int? OccurrenceId { get; init; }
    public int? ReplyToId { get; init; }
    public DateTime CreatedAt { get; init; }
}
