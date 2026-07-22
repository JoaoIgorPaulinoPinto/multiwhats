using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.dtos.Responses;

public record TaskListResponse
{
    public int Id { get; init; }
    public string Title { get; init; } = null!;
    public ClientTaskStatus Status { get; init; }
    public Priority Priority { get; init; }
    public DateTime? DueDate { get; init; }
    public string? ClientName { get; init; }
    public string? AssignedToName { get; init; }
    public DateTime CreatedAt { get; init; }
}
