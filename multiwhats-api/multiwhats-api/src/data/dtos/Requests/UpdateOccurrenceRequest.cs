using System.ComponentModel.DataAnnotations;
using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.dtos.Requests;

public record UpdateOccurrenceRequest
{
    [MaxLength(200)]
    public string? Title { get; init; }

    [MaxLength(2000)]
    public string? Description { get; init; }

    public OccurrenceStatus? Status { get; init; }

    public Priority? Priority { get; init; }

    public int? AssignedToUserId { get; init; }
}
