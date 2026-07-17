using System.ComponentModel.DataAnnotations;

namespace multiwhats_api.src.data.dtos.Requests;

public record LoginRequest
{
    [Required(ErrorMessage = "O nome de usuário é obrigatório")]
    public string Nome { get; init; } = null!;

    [Required(ErrorMessage = "A senha é obrigatória")]
    public string Senha { get; init; } = null!;
}
