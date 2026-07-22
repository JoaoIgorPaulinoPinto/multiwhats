using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.strategies;

public interface IMessageStrategy
{
    MessageType Type { get; }
    object BuildNodePayload(string jid, SendMessageRequest request);
    (string? body, bool hasMedia, string? mediaUrl, string? mediaMimeType, string? mediaFilename, long? mediaSize, string? mediaCaption) BuildMessageFields(SendMessageRequest request);
}
