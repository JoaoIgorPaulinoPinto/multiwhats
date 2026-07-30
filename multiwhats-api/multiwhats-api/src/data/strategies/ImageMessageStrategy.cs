using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.strategies;

// Strategy for image messages (JPEG, PNG, etc.) with optional caption.
public class ImageMessageStrategy : IMessageStrategy
{
    public MessageType Type => MessageType.Image;

    public object BuildNodePayload(string jid, SendMessageRequest request, string? userName = null)
    {
        var caption = !string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(request.MediaCaption)
            ? $"_*{userName}*_\n{request.MediaCaption}"
            : request.MediaCaption;

        return new
        {
            jid,
            mensagem = request.Text,
            type = "image",
            mediaBase64 = request.MediaBase64,
            mediaMimeType = request.MediaMimeType ?? "image/jpeg",
            caption
        };
    }

    // Extracts DB fields: body uses caption or text, hasMedia=true.
    public (string? body, bool hasMedia, string? mediaUrl, string? mediaMimeType, string? mediaFilename, long? mediaSize, string? mediaCaption) BuildMessageFields(SendMessageRequest request, string? userName = null)
    {
        var caption = !string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(request.MediaCaption)
            ? $"_*{userName}*_\n{request.MediaCaption}"
            : request.MediaCaption;
        return (
            body: caption ?? request.Text,
            hasMedia: true,
            mediaUrl: request.MediaBase64,
            mediaMimeType: request.MediaMimeType ?? "image/jpeg",
            mediaFilename: request.MediaFilename,
            mediaSize: EstimateBase64Size(request.MediaBase64),
            mediaCaption: request.MediaCaption
        );
    }

    // Estimates real file size from Base64 string length (base64 * 0.75). Strips "data:..." prefix.
    private static long? EstimateBase64Size(string? base64)
    {
        if (string.IsNullOrEmpty(base64)) return null;
        var data = base64.Contains(",") ? base64.Split(',')[1] : base64;
        return (long)(data.Length * 0.75);
    }
}
