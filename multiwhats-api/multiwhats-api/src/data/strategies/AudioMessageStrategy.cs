using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.strategies;

/// <summary>
/// STRATEGY PARA MENSAGENS DE ÁUDIO.
/// 
/// Envia áudios no formato OGG/Opus (padrão do WhatsApp).
/// 
/// Payload para o Node.js:
/// {
///   "jid": "5511999999999@c.us",
///   "mensagem": "Nota de voz",
///   "type": "audio",
///   "mediaBase64": "data:audio/ogg;base64,...",
///   "mediaMimeType": "audio/ogg; codecs=opus"
/// }
/// 
/// DIFERENÇA DO ÁUDIO PARA OUTROS TIPOS:
/// - Áudio NÃO tem "caption" (legenda) - o WhatsApp mostra apenas o player de áudio
/// - O body é o texto que aparece junto (geralmente vazio ou descrição)
/// - O formato padrão é OGG com codec Opus (padrão WhatsApp)
/// </summary>
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

    /// <summary>
    /// Extrai campos para o banco.
    /// - body: o texto (geralmente vazio para áudio)
    /// - hasMedia: true
    /// - mediaCaption: null (áudio não tem legenda)
    /// </summary>
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
            mediaCaption: null                                       // Áudio não tem legenda
        );
    }

    private static long? EstimateBase64Size(string? base64)
    {
        if (string.IsNullOrEmpty(base64)) return null;
        var data = base64.Contains(",") ? base64.Split(',')[1] : base64;
        return (long)(data.Length * 0.75);
    }
}
