using System.ComponentModel.DataAnnotations;

namespace multiwhats_api.src.data.dtos.Requests;

public record UpdateContactRequest
{
    [MaxLength(150)]
    public string? Name { get; init; }

    [MaxLength(150)]
    public string? PushName { get; init; }

    public bool? IsBlocked { get; init; }
}
