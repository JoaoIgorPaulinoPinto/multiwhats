
using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.dtos.Responses
{
    public record LastMessageResponse
    {
        public MessageType Type { get; init; }
        public string? Body { get; init; }
        public MessageDirection Direction { get; init; }
        public DeliveryStatus DeliveryStatus { get; init; }
    }
}
