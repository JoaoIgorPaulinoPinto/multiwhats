using System.ComponentModel.DataAnnotations;

namespace multiwhats_api.src.data.dtos.Requests;

public record RegisterUserRequest
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    [MaxLength(EntityConstraints.UserNameMaxLength)]
    public string Name { get; init; } = null!;

    [Required(ErrorMessage = "A senha é obrigatória")]
    public string Password { get; init; } = null!;

    public string? RegistrationCode { get; init; }
}
