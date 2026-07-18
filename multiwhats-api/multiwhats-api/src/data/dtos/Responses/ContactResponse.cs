namespace multiwhats_api.src.data.dtos.Responses;

public record ContactResponse
{
    public int Id { get; init; }
    public string Jid { get; init; } = null!;
    public string PhoneNumber { get; init; } = null!;
    public string? Name { get; init; }
    public string? PushName { get; init; }
    public string? ProfilePicUrl { get; init; }
    public bool IsBlocked { get; init; }
    public bool IsGroup { get; init; }
    public DateTime? LastMessageAt { get; init; }
    public int? ClientId { get; init; }
    public string? ClientName { get; init; }
    public int? GroupId { get; init; }
    public string? GroupName { get; init; }
    public int? CreatedByUserId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime LastUpdate { get; init; }
}
