using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace multiwhats_api.src.data.entities;

[Table("Ocorrencias")]
public class Ocorrencia
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; private set; }

    [Required(ErrorMessage = "O nome da ocorrência é obrigatório")]
    [MaxLength(200)]
    [Column("nome")]
    public string Nome { get; private set; }

    [MaxLength(1000)]
    [Column("descricao")]
    public string? Descricao { get; private set; }

    [Column("status_id")]
    public string? StatusId { get; private set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; private set; }

    [Required]
    [Column("created_at")]
    public Status status { get; private set; } 

    private Ocorrencia() { }

    public Ocorrencia(string nome, string? descricao)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Descricao = descricao;
        CreatedAt = DateTime.UtcNow;
    }
}
