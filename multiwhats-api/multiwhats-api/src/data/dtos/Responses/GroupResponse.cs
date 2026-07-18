namespace multiwhats_api.src.data.dtos.Responses;

public record GroupResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public string? WhatsAppGroupId { get; init; }
    public int MemberCount { get; init; }
    public DateTime CreatedAt { get; init; }
}
