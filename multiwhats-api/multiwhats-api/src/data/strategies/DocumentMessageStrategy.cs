using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.strategies;

/// <summary>
/// STRATEGY PARA MENSAGENS DE DOCUMENTO (PDF, Word, etc.).
/// 
/// Envia arquivos de documento com nome de arquivo.
/// 
/// Payload para o Node.js:
/// {
///   "jid": "5511999999999@c.us",
///   "mensagem": "Olá",
///   "type": "document",
///   "mediaBase64": "data:application/pdf;base64,...",
///   "mediaMimeType": "application/pdf",
///   "filename": "relatorio.pdf"
/// }
/// 
/// DIFERENÇA DOS DOCUMENTOS:
/// - O body usa o NOME DO ARQUIVO (não o texto ou legenda)
/// - Exemplo: se enviar "relatorio.pdf", o body será "relatorio.pdf"
/// - O formato padrão é PDF (mais comum para documentos)
/// </summary>
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
            mediaMimeType = request.MediaMimeType ?? "application/pdf",  // Padrão: PDF
            filename = request.MediaFilename ?? "document"               // Nome do arquivo ou "document"
        };
    }

    /// <summary>
    /// Extrai campos para o banco.
    /// - body: o NOME DO ARQUIVO (ex: "relatorio.pdf")
    /// - hasMedia: true
    /// - mediaCaption: null (documentos não têm legenda)
    /// </summary>
    public (string? body, bool hasMedia, string? mediaUrl, string? mediaMimeType, string? mediaFilename, long? mediaSize, string? mediaCaption) BuildMessageFields(SendMessageRequest request)
    {
        return (
            body: request.MediaFilename ?? request.Text,               // Nome do arquivo ou texto
            hasMedia: true,
            mediaUrl: request.MediaBase64,
            mediaMimeType: request.MediaMimeType ?? "application/pdf",
            mediaFilename: request.MediaFilename,
            mediaSize: EstimateBase64Size(request.MediaBase64),
            mediaCaption: null                                         // Documentos não têm legenda
        );
    }

    private static long? EstimateBase64Size(string? base64)
    {
        if (string.IsNullOrEmpty(base64)) return null;
        var data = base64.Contains(",") ? base64.Split(',')[1] : base64;
        return (long)(data.Length * 0.75);
    }
}
