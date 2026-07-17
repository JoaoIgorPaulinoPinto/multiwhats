using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace multiwhats_api.src.data.entities;

[Table("Ocorrencias")]
public class Ocorrencia : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; private set; }

    [Required(ErrorMessage = "O nome da ocorrência é obrigatório")]
    [MaxLength(200)]
    [Column("nome")]
    public string Nome { get; private set; } = null!;

    [MaxLength(1000)]
    [Column("descricao")]
    public string? Descricao { get; private set; }

    [Column("status_id")]
    public int? StatusId { get; private set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    [Required]
    [Column("last_update")]
    public DateTime LastUpdate { get; private set; }

    [ForeignKey(nameof(StatusId))]
    public Status? Status { get; private set; }

    private Ocorrencia() { }

    public Ocorrencia(string nome, string? descricao, int? statusId = null)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Descricao = descricao;
        StatusId = statusId;
        CreatedAt = DateTime.UtcNow;
    }
}
