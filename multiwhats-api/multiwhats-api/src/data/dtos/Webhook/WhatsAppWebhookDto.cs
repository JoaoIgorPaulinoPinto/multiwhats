using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.dtos.Webhook;

public record WhatsAppWebhookDto
{
    public string From { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string? Body { get; init; }
    public long Timestamp { get; init; }
    public string? NotifyName { get; init; }
    public string? MessageType { get; init; }
    public bool HasMedia { get; init; }
    public string? MediaUrl { get; init; }
    public string? MediaMimeType { get; init; }
    public string? MediaFilename { get; init; }
    public long? MediaSize { get; init; }
    public string? MediaCaption { get; init; }
    public string? MessageId { get; init; }
    public bool IsForwarded { get; init; }
    public int UserId { get; init; }
}
