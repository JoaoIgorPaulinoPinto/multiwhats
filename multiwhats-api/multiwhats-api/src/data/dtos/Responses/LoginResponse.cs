namespace multiwhats_api.src.data.dtos.Responses;

public record LoginResponse
{
    public string Token { get; init; } = null!;
    public UserResponse User { get; init; } = null!;
}
