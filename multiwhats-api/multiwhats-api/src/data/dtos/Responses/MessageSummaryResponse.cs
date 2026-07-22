using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.dtos.Responses;

public record MessageSummaryResponse
{
    public int Id { get; init; }
    public string? Body { get; init; }
    public MessageDirection Direction { get; init; }
    public MessageType Type { get; init; }
    public DateTime SentAt { get; init; }
    public string? PhoneNumber { get; init; }
    public string? NotifyName { get; init; }
    public bool HasMedia { get; init; }
    public string? MediaMimeType { get; init; }
    public DeliveryStatus DeliveryStatus { get; init; }
    public int ChatId { get; init; }
    public DateTime CreatedAt { get; init; }
}
