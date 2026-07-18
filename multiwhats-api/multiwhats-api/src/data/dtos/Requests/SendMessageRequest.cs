using System.ComponentModel.DataAnnotations;

namespace multiwhats_api.src.data.dtos.Requests;

public record SendMessageRequest
{
    [Required(ErrorMessage = "O número de destino é obrigatório")]
    public string PhoneNumber { get; init; } = null!;

    [Required(ErrorMessage = "O conteúdo da mensagem é obrigatório")]
    public string Text { get; init; } = null!;
}
