using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.strategies;

/// <summary>
/// STRATEGY PARA MENSAGENS DE IMAGEM.
/// 
/// Envia imagens (JPEG, PNG, etc.) com legenda opcional.
/// 
/// Payload para o Node.js:
/// {
///   "jid": "5511999999999@c.us",
///   "mensagem": "Olá",
///   "type": "image",
///   "mediaBase64": "data:image/jpeg;base64,/9j/4AAQ...",
///   "mediaMimeType": "image/jpeg",
///   "caption": "Legenda da imagem"
/// }
/// 
/// Campos no banco:
/// - body: legenda ou texto (o que aparece como "texto" da mensagem)
/// - hasMedia: true (tem arquivo de mídia)
/// - mediaUrl: base64 da imagem (armazenado como LONGTEXT no MySQL)
/// - mediaMimeType: "image/jpeg" (tipo padrão se não informado)
/// - mediaSize: tamanho estimado em bytes (base64 * 0.75)
/// 
/// CÁLCULO DO TAMANHO BASE64:
/// - Base64 codifica 3 bytes em 4 caracteres
/// - Então: tamanho real = tamanho do base64 * 0.75
/// - Exemplo: base64 de 1000 caracteres ≈ 750 bytes reais
/// </summary>
public class ImageMessageStrategy : IMessageStrategy
{
    public MessageType Type => MessageType.Image;

    /// <summary>
    /// Monta o payload JSON para enviar imagem ao Node.js.
    /// Inclui: JID, texto, tipo "image", base64 da mídia, MIME type, e legenda.
    /// </summary>
    public object BuildNodePayload(string jid, SendMessageRequest request)
    {
        return new
        {
            jid,
            mensagem = request.Text,
            type = "image",
            mediaBase64 = request.MediaBase64,
            mediaMimeType = request.MediaMimeType ?? "image/jpeg",  // Padrão: JPEG
            caption = request.MediaCaption
        };
    }

    /// <summary>
    /// Extrai campos para o banco.
    /// - body: usa a legenda (MediaCaption) ou o texto como corpo da mensagem
    /// - hasMedia: true (sempre tem mídia para imagem)
    /// - mediaUrl: armazena o base64 completo da imagem
    /// - mediaMimeType: tipo MIME da imagem
    /// - mediaSize: tamanho estimado via EstimateBase64Size()
    /// </summary>
    public (string? body, bool hasMedia, string? mediaUrl, string? mediaMimeType, string? mediaFilename, long? mediaSize, string? mediaCaption) BuildMessageFields(SendMessageRequest request)
    {
        return (
            body: request.MediaCaption ?? request.Text,           // Legenda ou texto
            hasMedia: true,
            mediaUrl: request.MediaBase64,                        // Base64 completo
            mediaMimeType: request.MediaMimeType ?? "image/jpeg", // Padrão: JPEG
            mediaFilename: request.MediaFilename,
            mediaSize: EstimateBase64Size(request.MediaBase64),   // Tamanho estimado
            mediaCaption: request.MediaCaption
        );
    }

    /// <summary>
    /// Estima o tamanho REAL de um arquivo a partir do seu conteúdo Base64.
    /// 
    /// POR QUE É NECESSÁRIO:
    /// - O base64 é uma representação em texto de dados binários
    /// - Cada 3 bytes viram 4 caracteres em base64
    /// - Então: tamanho real ≈ tamanho do base64 × 0.75
    /// 
    /// EXEMPLO:
    /// - Uma imagem de 1MB em base64 tem ~1.333.333 caracteres
    /// - 1.333.333 × 0.75 = 1.000.000 bytes = 1MB (correto!)
    /// 
    /// OBS: Se o base64 tiver o prefixo "data:image/jpeg;base64,", ele é removido
    /// antes do cálculo (pois o prefixo não faz parte dos dados reais).
    /// </summary>
    private static long? EstimateBase64Size(string? base64)
    {
        if (string.IsNullOrEmpty(base64)) return null;
        var data = base64.Contains(",") ? base64.Split(',')[1] : base64;  // Remove prefixo "data:..."
        return (long)(data.Length * 0.75);                                  // Calcula tamanho real
    }
}
