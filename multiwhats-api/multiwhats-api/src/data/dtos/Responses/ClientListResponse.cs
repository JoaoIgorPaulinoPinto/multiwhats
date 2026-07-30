using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.dtos.Responses;

public record ClientListResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string? MainPhoneNumber { get; init; }
    public ClientStatus Status { get; init; }
    public int ContactCount { get; init; }
    public DateTime CreatedAt { get; init; }
}
