using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace multiwhats_api.src.data.entities;

[Table("Contatos")]
public class Contato
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; private set; }

    [Required(ErrorMessage = "O nome do contato é obrigatório")]
    [MaxLength(150)]
    [Column("nome")]
    public string Nome { get; private set; }

    [Required(ErrorMessage = "O número é obrigatório")]
    [MaxLength(20)]
    [Column("numero")]
    public string Numero { get; private set; }

    [Column("grupo_id")]
    public int GrupoId { get; private set; }

    [Column("ocorrencia_atual_id")]
    public int OcorrenciaAtualId { get; private set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; private set; }

    [ForeignKey(nameof(OcorrenciaAtualId))]
    public Ocorrencia? OcorrenciaAtual { get; private set; }

    public List<Mensagem>? Mensagens { get; private set; }

    [ForeignKey(nameof(GrupoId))]
    public Grupo? Grupo { get; private set; }

    private Contato() { }

    public Contato(string nome, string numero, int grupoId, int ocorrenciaAtualId)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Numero = numero ?? throw new ArgumentNullException(nameof(numero));
        GrupoId = grupoId;
        OcorrenciaAtualId = ocorrenciaAtualId;
        CreatedAt = DateTime.UtcNow;
    }
}
