using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.dtos.Responses;

public record UserResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string Role { get; init; } = UserRole.Support.ToString();
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}
