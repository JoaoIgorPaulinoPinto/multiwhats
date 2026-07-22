using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.strategies;

public class DocumentMessageStrategy : IMessageStrategy
{
    public MessageType Type => MessageType.Document;

    public object BuildNodePayload(string jid, SendMessageRequest request)
    {
        return new
        {
            jid,
            mensagem = request.Text,
            type = "document",
            mediaBase64 = request.MediaBase64,
            mediaMimeType = request.MediaMimeType ?? "application/pdf",
            filename = request.MediaFilename ?? "document"
        };
    }

    public (string? body, bool hasMedia, string? mediaUrl, string? mediaMimeType, string? mediaFilename, long? mediaSize, string? mediaCaption) BuildMessageFields(SendMessageRequest request)
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
