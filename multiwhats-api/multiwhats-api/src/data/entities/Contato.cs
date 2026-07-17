using multiwhats_api.src.data.db;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace multiwhats_api.src.data.entities;

[Table("Contatos")]
public class Contato : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; private set; }

    [Required(ErrorMessage = "O nome do contato é obrigatório")]
    [MaxLength(150)]
    [Column("nome")]
    public string Nome { get; private set; } = null!;

    [Required(ErrorMessage = "O número é obrigatório")]
    [MaxLength(20)]
    [Column("numero")]
    public string Numero { get; private set; } = null!;

    [Column("grupo_id")]
    public int? GrupoId { get; private set; } = null;

    [Column("ocorrencia_atual_id")]
    public int? OcorrenciaAtualId { get; private set; }

    [Column("criado_por")]
    public int CriadoPor { get; private set; }
    [ForeignKey(nameof(OcorrenciaAtualId))]
    public Ocorrencia? OcorrenciaAtual { get; private set; }

    public ICollection<Mensagem> Mensagens { get; private set; } = new List<Mensagem>();

    [ForeignKey(nameof(GrupoId))]
    public Grupo? Grupo { get; private set; }

    private Contato() { }

    public Contato(string nome, string numero, int grupoId, int? ocorrenciaAtualId = null)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Numero = numero ?? throw new ArgumentNullException(nameof(numero));
        GrupoId = grupoId;
        OcorrenciaAtualId = ocorrenciaAtualId;
        CreatedAt = DateTime.UtcNow;
    }

    public Contato(string nome, string numero, int? grupoId, int? ocorrenciaAtualId, int criadoPor)
    {
        Nome = nome;
        Numero = numero;
        GrupoId = grupoId;
        CriadoPor = criadoPor;
        OcorrenciaAtualId = ocorrenciaAtualId;
    }
}
