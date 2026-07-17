using System.ComponentModel.DataAnnotations;

namespace multiwhats_api.src.data.dtos.Requests;

public record CriarContatoRequest
{
    [Required(ErrorMessage = "O nome do contato é obrigatório")]
    [MaxLength(150)]
    public string Nome { get; init; } = null!;

    [Required(ErrorMessage = "O número é obrigatório")]
    [MaxLength(20)]
    public string Numero { get; init; } = null!;
    public int? GrupoId { get; init; }
    public int? OcorrenciaAtualId { get; init; }
}
