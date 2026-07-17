namespace multiwhats_api.src.data.dtos.Responses;

public record UsuarioResponse
{
    public int Id { get; init; }
    public string Nome { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string? Telefone { get; init; }
    public DateTime CreatedAt { get; init; }
}
