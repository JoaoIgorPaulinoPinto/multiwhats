using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.strategies;

// Strategy for text messages. No media, only text.
public class TextMessageStrategy : IMessageStrategy
{
    public MessageType Type => MessageType.Text;

    public object BuildNodePayload(string jid, SendMessageRequest request, string? userName = null)
    {
        var mensagem = !string.IsNullOrEmpty(userName)
            ? $"_*{userName}*_\n{request.Text}"
            : request.Text;

        return new
        {
            jid,
            mensagem,
            type = "text"
        };
    }

    // Extracts DB fields: body=text, hasMedia=false, media=null.
    public (string? body, bool hasMedia, string? mediaUrl, string? mediaMimeType, string? mediaFilename, long? mediaSize, string? mediaCaption) BuildMessageFields(SendMessageRequest request, string? userName = null)
    {
        var body = !string.IsNullOrEmpty(userName)
            ? $"_*{userName}*_\n{request.Text}"
            : request.Text;
        return (body, false, null, null, null, null, null);
    }
}
