namespace multiwhats_api.src.data.dtos.Requests;

public record LogoutRequest
{
    public string? RefreshToken { get; init; }
}
