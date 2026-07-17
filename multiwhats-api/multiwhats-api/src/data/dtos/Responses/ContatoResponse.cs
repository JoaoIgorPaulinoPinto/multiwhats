namespace multiwhats_api.src.data.dtos.Responses;

public record ContatoResponse
{
    public int Id { get; init; }
    public string Nome { get; init; } = null!;
    public string Numero { get; init; } = null!;
    public int? GrupoId { get; init; }
    public int? OcorrenciaAtualId { get; init; }
}
