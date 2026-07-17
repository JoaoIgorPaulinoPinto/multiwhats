namespace multiwhats_api.src.data.dtos.Responses;

public record LoginResponse
{
    public string Token { get; init; } = null!;
    public UsuarioResponse Usuario { get; init; } = null!;
}
