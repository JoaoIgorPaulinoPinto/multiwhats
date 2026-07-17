using System.ComponentModel.DataAnnotations;

namespace multiwhats_api.src.data.dtos.Requests;

public record RegistrarUsuarioRequest
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    [MaxLength(200)]
    public string Nome { get; init; } = null!;

    [Required(ErrorMessage = "A senha é obrigatória")]
    [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres")]
    public string Senha { get; init; } = null!;
}
