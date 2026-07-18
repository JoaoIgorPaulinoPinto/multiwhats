using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.dtos.Responses;

public record OccurrenceResponse
{
    public int Id { get; init; }
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public OccurrenceStatus Status { get; init; }
    public Priority Priority { get; init; }
    public int ContactId { get; init; }
    public int? AssignedToUserId { get; init; }
    public string? AssignedToName { get; init; }
    public int? CreatedByUserId { get; init; }
    public string? CreatedByName { get; init; }
    public int MessageCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime LastUpdate { get; init; }
}
