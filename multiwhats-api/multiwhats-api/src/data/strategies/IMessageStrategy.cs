using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.strategies;

// Strategy pattern interface for WhatsApp message types.
public interface IMessageStrategy
{
    // The message type this strategy handles.
    MessageType Type { get; }

    // Builds the JSON payload sent to Node.js.
    object BuildNodePayload(string jid, SendMessageRequest request, string? userName = null);

    // Extracts message fields for DB storage: (body, hasMedia, mediaUrl, mediaMimeType, mediaFilename, mediaSize, mediaCaption).
    (string? body, bool hasMedia, string? mediaUrl, string? mediaMimeType, string? mediaFilename, long? mediaSize, string? mediaCaption) BuildMessageFields(SendMessageRequest request, string? userName = null);
}
