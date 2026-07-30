namespace multiwhats_api.src.data.dtos.Responses;

public record ContactListResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string PhoneNumber { get; init; } = null!;
    public string? PushName { get; init; }
    public bool IsBlocked { get; init; }
    public bool IsGroup { get; init; }
    public string? ClientName { get; init; }
    public DateTime CreatedAt { get; init; }
}
