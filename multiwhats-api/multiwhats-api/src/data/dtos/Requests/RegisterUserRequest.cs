using System.ComponentModel.DataAnnotations;

namespace multiwhats_api.src.data.dtos.Requests;

public record RegisterUserRequest
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    [MaxLength(200)]
    public string Name { get; init; } = null!;

    [Required(ErrorMessage = "A senha é obrigatória")]
    [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres")]
    public string Password { get; init; } = null!;
}
