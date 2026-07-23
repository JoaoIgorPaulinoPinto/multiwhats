using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.strategies;

/// <summary>
/// STRATEGY PARA STICKERS (FIGURINHAS).
/// 
/// Envia stickers no formato WebP (padrão WhatsApp).
/// 
/// Payload para o Node.js:
/// {
///   "jid": "5511999999999@c.us",
///   "type": "sticker",
///   "mediaBase64": "data:image/webp;base64,...",
///   "mediaMimeType": "image/webp"
/// }
/// 
/// PECULIARIDADE DO STICKER:
/// - NÃO tem "mensagem" (texto) - stickers são apenas imagens animadas
/// - body = null (não tem texto)
/// - mediaCaption = null (não tem legenda)
/// - O formato padrão é WebP (formato de imagem compacto do WhatsApp)
/// </summary>
public class StickerMessageStrategy : IMessageStrategy
{
    public MessageType Type => MessageType.Sticker;

    public object BuildNodePayload(string jid, SendMessageRequest request)
    {
        return new
        {
            jid,
            type = "sticker",
            mediaBase64 = request.MediaBase64,
            mediaMimeType = request.MediaMimeType ?? "image/webp"  // Padrão: WebP
            // NOTA: Sticker NÃO envia "mensagem" - stickers são apenas imagens
        };
    }

    /// <summary>
    /// Extrai campos para o banco.
    /// - body: null (stickers não têm texto)
    /// - hasMedia: true
    /// - mediaCaption: null (stickers não têm legenda)
    /// </summary>
    public (string? body, bool hasMedia, string? mediaUrl, string? mediaMimeType, string? mediaFilename, long? mediaSize, string? mediaCaption) BuildMessageFields(SendMessageRequest request)
    {
        return (
            body: null,                                              // Stickers não têm texto
            hasMedia: true,
            mediaUrl: request.MediaBase64,
            mediaMimeType: request.MediaMimeType ?? "image/webp",   // Padrão: WebP
            mediaFilename: request.MediaFilename,
            mediaSize: EstimateBase64Size(request.MediaBase64),
            mediaCaption: null                                       // Stickers não têm legenda
        );
    }

    private static long? EstimateBase64Size(string? base64)
    {
        if (string.IsNullOrEmpty(base64)) return null;
        var data = base64.Contains(",") ? base64.Split(',')[1] : base64;
        return (long)(data.Length * 0.75);
    }
}
