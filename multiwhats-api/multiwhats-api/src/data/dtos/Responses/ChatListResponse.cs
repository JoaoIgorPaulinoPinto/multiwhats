namespace multiwhats_api.src.data.dtos.Responses;

public record ChatListResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string? PhoneNumber { get; init; }
    public string? ContactName { get; init; }
    public string? ClientName { get; init; }
    public DateTime? LastMessageAt { get; init; }
    public string? LastMessageBody { get; init; }
    public string? AssignedToUserName { get; init; }
    public int MessageCount { get; init; }
    public int OccurrenceCount { get; init; }
    public DateTime CreatedAt { get; init; }
}
