using System.ComponentModel.DataAnnotations;
using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.dtos.Requests;

public record SendMessageRequest
{
    [Required(ErrorMessage = "O JID de destino é obrigatório")]
    public string Jid { get; init; } = null!;

    public string? Text { get; init; }

    public MessageType Type { get; init; } = MessageType.Text;

    public string? MediaBase64 { get; init; }

    public string? MediaMimeType { get; init; }

    public string? MediaFilename { get; init; }

    public string? MediaCaption { get; init; }
}
