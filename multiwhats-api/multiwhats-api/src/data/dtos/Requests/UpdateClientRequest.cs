using System.ComponentModel.DataAnnotations;
using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.dtos.Requests;

public record UpdateClientRequest
{
    [MaxLength(200)]
    public string? Name { get; init; }

    [MaxLength(20)]
    public string? MainPhoneNumber { get; init; }

    public ClientStatus? Status { get; init; }
}
