using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.strategies;

/// <summary>
/// STRATEGY PARA MENSAGENS DE TEXTO.
/// 
/// Esta é a strategy mais simples. Não envia mídia, apenas texto.
/// 
/// Payload para o Node.js:
/// { "jid": "5511999999999@c.us", "mensagem": "Olá", "type": "text" }
/// 
/// Campos no banco:
/// - body: "Olá" (o texto da mensagem)
/// - hasMedia: false (não tem mídia)
/// - mídia: tudo null
/// </summary>
public class TextMessageStrategy : IMessageStrategy
{
    public MessageType Type => MessageType.Text;

    /// <summary>
    /// Monta o payload JSON para enviar texto ao Node.js.
    /// Formato: { "jid": "...", "mensagem": "texto", "type": "text" }
    /// </summary>
    public object BuildNodePayload(string jid, SendMessageRequest request)
    {
        return new
        {
            jid,
            mensagem = request.Text,
            type = "text"
        };
    }

    /// <summary>
    /// Extrai campos para o banco. Para texto:
    /// - body = o texto digitado
    /// - hasMedia = false (não tem arquivo de mídia)
    /// - mídia = null (tudo)
    /// </summary>
    public (string? body, bool hasMedia, string? mediaUrl, string? mediaMimeType, string? mediaFilename, long? mediaSize, string? mediaCaption) BuildMessageFields(SendMessageRequest request)
    {
        return (request.Text, false, null, null, null, null, null);
    }
}
