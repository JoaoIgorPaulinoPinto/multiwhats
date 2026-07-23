using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.strategies;

/// <summary>
/// STRATEGY PARA MENSAGENS DE VÍDEO.
/// 
/// Envia vídeos no formato MP4 com legenda opcional.
/// 
/// Payload para o Node.js:
/// {
///   "jid": "5511999999999@c.us",
///   "mensagem": "Olá",
///   "type": "video",
///   "mediaBase64": "data:video/mp4;base64,...",
///   "mediaMimeType": "video/mp4",
///   "caption": "Legenda do vídeo"
/// }
/// 
/// COMPORTAMENTO:
/// - Similar à imagem: body usa a legenda ou texto
/// - O formato padrão é MP4 (padrão do WhatsApp para vídeos)
/// </summary>
public class VideoMessageStrategy : IMessageStrategy
{
    public MessageType Type => MessageType.Video;

    public object BuildNodePayload(string jid, SendMessageRequest request)
    {
        return new
        {
            jid,
            mensagem = request.Text,
            type = "video",
            mediaBase64 = request.MediaBase64,
            mediaMimeType = request.MediaMimeType ?? "video/mp4",  // Padrão: MP4
            caption = request.MediaCaption
        };
    }

    public (string? body, bool hasMedia, string? mediaUrl, string? mediaMimeType, string? mediaFilename, long? mediaSize, string? mediaCaption) BuildMessageFields(SendMessageRequest request)
    {
        return (
            body: request.MediaCaption ?? request.Text,
            hasMedia: true,
            mediaUrl: request.MediaBase64,
            mediaMimeType: request.MediaMimeType ?? "video/mp4",
            mediaFilename: request.MediaFilename,
            mediaSize: EstimateBase64Size(request.MediaBase64),
            mediaCaption: request.MediaCaption
        );
    }

    private static long? EstimateBase64Size(string? base64)
    {
        if (string.IsNullOrEmpty(base64)) return null;
        var data = base64.Contains(",") ? base64.Split(',')[1] : base64;
        return (long)(data.Length * 0.75);
    }
}
