using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace multiwhats_api.src.data.entities;

[Table("mensagens")]
public class Mensagem : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; private set; }

    [Required(ErrorMessage = "O remetente é obrigatório")]
    [MaxLength(50)]
    [Column("remetente")]
    public string From { get; private set; } = null!;

    [Required(ErrorMessage = "O corpo da mensagem é obrigatório")]
    [Column("corpo")]
    public string Body { get; private set; } = null!;

    [Column("timestamp")]
    public long Timestamp { get; private set; }

    [MaxLength(100)]
    [Column("nome_notificacao")]
    public string? NotifyName { get; private set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; private set; }

    [Column("contato_id")]
    public int? ContatoId { get; private set; }

    [ForeignKey(nameof(ContatoId))]
    public Contato? Contato { get; private set; }

    private Mensagem() { }

    public Mensagem(string from, string body, long timestamp, string? notifyName, int? contatoId = null)
    {
        From = from ?? throw new ArgumentNullException(nameof(from));
        Body = body ?? throw new ArgumentNullException(nameof(body));
        Timestamp = timestamp;
        NotifyName = notifyName;
        ContatoId = contatoId;
        CreatedAt = DateTime.UtcNow;
    }
}
