namespace multiwhats_api.src.data.dtos.Responses;

public record ChatDetailResponse
{
    public int Id { get; init; }
    public string Jid { get; init; } = null!;
    public string? PhoneNumber { get; init; }
    public string? Name { get; init; }
    public int? ContactId { get; init; }
    public string? ContactName { get; init; }
    public int? ClientId { get; init; }
    public string? ClientName { get; init; }
    public DateTime? LastMessageAt { get; init; }
    public string? LastMessageBody { get; init; }
    public List<OccurrenceDetailResponse>? Occurrences { get; init; }
    public int? AssignedToUserId { get; init; }
    public string? AssignedToUserName { get; init; }
    public int? CreatedByUserId { get; init; }
    public int MessageCount { get; init; }
    public int OccurrenceCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime LastUpdate { get; init; }
}
