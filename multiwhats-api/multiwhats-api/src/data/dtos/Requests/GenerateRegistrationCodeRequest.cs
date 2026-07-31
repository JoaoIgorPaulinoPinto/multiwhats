using System.ComponentModel.DataAnnotations;

namespace multiwhats_api.src.data.dtos.Requests;

public record GenerateRegistrationCodeRequest
{
    [Range(1, 100)]
    public int Quantity { get; init; } = 1;
}
