namespace multiwhats_api.src.data.dtos.Requests;

public record RefreshTokenRequest
{
    public string RefreshToken { get; init; } = null!;
}
