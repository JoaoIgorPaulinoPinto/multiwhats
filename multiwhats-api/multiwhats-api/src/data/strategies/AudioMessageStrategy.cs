using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.strategies;

// Strategy for audio messages (OGG/Opus format). No caption - WhatsApp shows only the audio player.
public class AudioMessageStrategy : IMessageStrategy
{
    public MessageType Type => MessageType.Audio;

    public object BuildNodePayload(string jid, SendMessageRequest request, string? userName = null)
    {
        var mensagem = !string.IsNullOrEmpty(userName)
            ? $"_*{userName}*_\n{request.Text}"
            : request.Text;

        return new
        {
            jid,
            mensagem,
            type = "audio",
            mediaBase64 = request.MediaBase64,
            mediaMimeType = request.MediaMimeType ?? "audio/ogg; codecs=opus"
        };
    }

    // Extracts DB fields: body text, hasMedia=true, mediaCaption=null (audio has no caption).
    public (string? body, bool hasMedia, string? mediaUrl, string? mediaMimeType, string? mediaFilename, long? mediaSize, string? mediaCaption) BuildMessageFields(SendMessageRequest request, string? userName = null)
    {
        var body = !string.IsNullOrEmpty(userName)
            ? $"_* {userName} *_\n{request.Text}"
            : request.Text;
        return (
            body: body,
            hasMedia: true,
            mediaUrl: request.MediaBase64,
            mediaMimeType: request.MediaMimeType ?? "audio/ogg; codecs=opus",
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
