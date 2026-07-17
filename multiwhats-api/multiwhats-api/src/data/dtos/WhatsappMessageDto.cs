namespace multiwhats_api.src.data.dtos;

public class WhatsappMessageDto
{
    public string From { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public long Timestamp { get; set; }
    public string NotifyName { get; set; } = string.Empty;
}
