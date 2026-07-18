using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.dtos.Responses;

public record UserResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public UserRole Role { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}
