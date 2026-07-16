using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace multiwhats_api.src.data.entities;

[Table("Grupos")]
public class Grupo
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; private set; }

    [Required(ErrorMessage = "O nome do grupo é obrigatório")]
    [MaxLength(200)]
    [Column("nome")]
    public string Nome { get; private set; }

    [MaxLength(500)]
    [Column("descricao")]
    public string Descricao { get; private set; }

    public ICollection<Contato> Membros { get; private set; } = new List<Contato>();

    private Grupo() { }

    public Grupo(string nome, string descricao)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Descricao = descricao ?? throw new ArgumentNullException(nameof(descricao));
    }
}
