using System.ComponentModel.DataAnnotations;
using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.dtos.Requests;

public record UpdateTaskRequest
{
    [MaxLength(300)]
    public string? Title { get; init; }

    [MaxLength(2000)]
    public string? Description { get; init; }

    public ClientTaskStatus? Status { get; init; }

    public Priority? Priority { get; init; }

    public DateTime? DueDate { get; init; }

    public int? AssignedToUserId { get; init; }
}
