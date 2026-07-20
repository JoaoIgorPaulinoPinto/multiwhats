using System.ComponentModel.DataAnnotations;

namespace multiwhats_api.src.data.dtos.Requests;

public record SendMessageRequest
{
    [Required(ErrorMessage = "O JID de destino é obrigatório")]
    public string Jid { get; init; } = null!;

    [Required(ErrorMessage = "O conteúdo da mensagem é obrigatório")]
    public string Text { get; init; } = null!;
}
