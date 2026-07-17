using System.ComponentModel.DataAnnotations;

namespace multiwhats_api.src.data.dtos.Requests;

public record LoginRequest
{
    [Required(ErrorMessage = "O e-mail é obrigatório")]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    public string Email { get; init; } = null!;

    [Required(ErrorMessage = "A senha é obrigatória")]
    public string Senha { get; init; } = null!;
}
