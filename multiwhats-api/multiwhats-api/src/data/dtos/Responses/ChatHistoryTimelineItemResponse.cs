namespace multiwhats_api.src.data.dtos.Responses;

// A single chronological entry in the chat history timeline.
public record ChatHistoryTimelineItemResponse
{
    public string Type { get; init; } = null!;
    public string Title { get; init; } = null!;
    public string Description { get; init; } = null!;
    public DateTime Timestamp { get; init; }
    public int? UserId { get; init; }
    public string? UserName { get; init; }
    public int? OccurrenceId { get; init; }
}
