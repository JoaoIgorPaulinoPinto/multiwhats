namespace multiwhats_api.src.data.dtos.Webhook;

public record WhatsappMessageDto
{
    public string From { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public long Timestamp { get; init; }
    public string NotifyName { get; init; } = string.Empty;
}
