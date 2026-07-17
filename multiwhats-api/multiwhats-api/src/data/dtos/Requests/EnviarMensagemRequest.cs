using System.ComponentModel.DataAnnotations;

namespace multiwhats_api.src.data.dtos.Requests;

public record EnviarMensagemRequest
{
    [Required(ErrorMessage = "O conteúdo da mensagem é obrigatório")]
    public string Conteudo { get; init; } = "";

    [Required(ErrorMessage = "O número de destino é obrigatório")]
    public string Numero { get; init; } = "";
}
