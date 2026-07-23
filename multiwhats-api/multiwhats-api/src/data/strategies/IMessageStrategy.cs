using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.strategies;

/// <summary>
/// INTERFACE DO PADRÃO STRATEGY PARA MENSAGENS.
/// 
/// Cada classe que implementa esta interface sabe como tratar um tipo específico
/// de mensagem WhatsApp (texto, imagem, áudio, etc.).
/// 
/// MÉTODOS:
/// - Type: retorna qual tipo de mensagem esta strategy trata
/// - BuildNodePayload: monta o JSON que o Node.js espera receber
/// - BuildMessageFields: extrai os campos para salvar no banco de dados
/// 
/// POR QUE USAR UMA INTERFACE:
/// - Permite trocar a implementação sem mudar quem usa
/// - O SendMessageUseCase não precisa saber como cada tipo funciona
/// - Ele só chama strategy.BuildNodePayload() e confia que vai funcionar
/// </summary>
public interface IMessageStrategy
{
    /// <summary>
    /// Retorna o tipo de mensagem que esta strategy trata.
    /// Exemplo: TextMessageStrategy retorna MessageType.Text
    /// </summary>
    MessageType Type { get; }

    /// <summary>
    /// Monta o payload JSON que será enviado para o Node.js.
    /// 
    /// O Node.js espera um formato diferente para cada tipo:
    /// - Texto: { "jid": "...", "mensagem": "Olá", "type": "text" }
    /// - Imagem: { "jid": "...", "type": "image", "mediaBase64": "...", "caption": "..." }
    /// - Áudio: { "jid": "...", "type": "audio", "mediaBase64": "..." }
    /// </summary>
    object BuildNodePayload(string jid, SendMessageRequest request);

    /// <summary>
    /// Extrai os campos da mensagem para salvar no banco de dados MySQL.
    /// Retorna uma tupla com: body, hasMedia, mediaUrl, mediaMimeType, mediaFilename, mediaSize, mediaCaption
    /// 
    /// Cada strategy define quais campos preencher:
    /// - Texto: body = texto, hasMedia = false, mídia = null
    /// - Imagem: body = caption ou texto, hasMedia = true, mediaUrl = base64
    /// - Áudio: body = texto, hasMedia = true, mediaUrl = base64
    /// </summary>
    (string? body, bool hasMedia, string? mediaUrl, string? mediaMimeType, string? mediaFilename, long? mediaSize, string? mediaCaption) BuildMessageFields(SendMessageRequest request);
}
