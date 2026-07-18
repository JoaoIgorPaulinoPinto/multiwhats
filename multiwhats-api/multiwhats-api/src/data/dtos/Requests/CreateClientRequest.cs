using System.ComponentModel.DataAnnotations;

namespace multiwhats_api.src.data.dtos.Requests;

public record CreateClientRequest
{
    [Required(ErrorMessage = "O nome do cliente é obrigatório")]
    [MaxLength(200)]
    public string Name { get; init; } = null!;

    [MaxLength(20)]
    public string? MainPhoneNumber { get; init; }
}
