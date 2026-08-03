using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.dtos.Webhook;

public record MessageDeliveryStatusDto
{
    public string? MessageId { get; init; }
    public DeliveryStatus DeliveryStatus { get; init; }
}
