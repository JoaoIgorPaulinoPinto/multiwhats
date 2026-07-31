using System.ComponentModel.DataAnnotations;

namespace multiwhats_api.src.data.dtos.Requests;

public record UpdateSystemParameterRequest
{
    [Required(ErrorMessage = "O valor é obrigatório")]
    public string Value { get; init; } = null!;
}
