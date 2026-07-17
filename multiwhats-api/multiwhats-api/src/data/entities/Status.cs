using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace multiwhats_api.src.data.entities;

[Table("Status")]
public class Status : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; private set; }

    [MaxLength(200)]
    [Column("descricao")]
    public string? Descricao { get; private set; }

    [Required]
    [Column("ativo")]
    public bool Ativo { get; private set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public ICollection<Ocorrencia> Ocorrencias { get; private set; } = new List<Ocorrencia>();

    private Status() { }

    public Status(string? descricao, bool ativo)
    {
        Descricao = descricao;
        Ativo = ativo;
    }
}
