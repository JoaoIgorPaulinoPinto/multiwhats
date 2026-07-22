using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.dtos.Responses;

public record OccurrenceListResponse
{
    public int Id { get; init; }
    public string Title { get; init; } = null!;
    public OccurrenceStatus Status { get; init; }
    public Priority Priority { get; init; }
    public string? ChatName { get; init; }
    public string? AssignedToName { get; init; }
    public int MessageCount { get; init; }
    public DateTime CreatedAt { get; init; }
}
