using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.strategies;

public class TextMessageStrategy : IMessageStrategy
{
    public MessageType Type => MessageType.Text;

    public object BuildNodePayload(string jid, SendMessageRequest request)
    {
        return new
        {
            jid,
            mensagem = request.Text,
            type = "text"
        };
    }

    public (string? body, bool hasMedia, string? mediaUrl, string? mediaMimeType, string? mediaFilename, long? mediaSize, string? mediaCaption) BuildMessageFields(SendMessageRequest request)
    {
        return (request.Text, false, null, null, null, null, null);
    }
}
