using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.strategies;

// Strategy for document messages (PDF, Word, etc.). Body uses filename, not text or caption.
public class DocumentMessageStrategy : IMessageStrategy
{
    public MessageType Type => MessageType.Document;

    public object BuildNodePayload(string jid, SendMessageRequest request, string? userName = null)
    {
        var mensagem = !string.IsNullOrEmpty(userName)
            ? $"_*{userName}*_\n{request.Text}"
            : request.Text;

        return new
        {
            jid,
            mensagem,
            type = "document",
            mediaBase64 = request.MediaBase64,
            mediaMimeType = request.MediaMimeType ?? "application/pdf",
            filename = request.MediaFilename ?? "document"
        };
    }

    // Extracts DB fields: body = filename, hasMedia=true, mediaCaption=null.
    public (string? body, bool hasMedia, string? mediaUrl, string? mediaMimeType, string? mediaFilename, long? mediaSize, string? mediaCaption) BuildMessageFields(SendMessageRequest request, string? userName = null)
    {
        return (
            body: request.MediaFilename ?? request.Text,
            hasMedia: true,
            mediaUrl: request.MediaBase64,
            mediaMimeType: request.MediaMimeType ?? "application/pdf",
            mediaFilename: request.MediaFilename,
            mediaSize: EstimateBase64Size(request.MediaBase64),
            mediaCaption: null
        );
    }

    private static long? EstimateBase64Size(string? base64)
    {
        if (string.IsNullOrEmpty(base64)) return null;
        var data = base64.Contains(",") ? base64.Split(',')[1] : base64;
        return (long)(data.Length * 0.75);
    }
}
