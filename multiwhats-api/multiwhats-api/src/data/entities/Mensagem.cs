using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace multiwhats_api.src.data.entities;

[Table("MessageLogs")]
public class Mensagem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("Id")]
    public int Id { get; private set; }

    [Required(ErrorMessage = "O remetente é obrigatório")]
    [MaxLength(50)]
    [Column("From")]
    public string From { get; private set; }

    [Required(ErrorMessage = "O corpo da mensagem é obrigatório")]
    [Column("Body")]
    public string Body { get; private set; }

    [Column("Timestamp")]
    public long Timestamp { get; private set; }

    [MaxLength(100)]
    [Column("NotifyName")]
    public string? NotifyName { get; private set; }

    [Required]
    [Column("CreatedAt")]
    public DateTime CreatedAt { get; private set; }

    private Mensagem() { }

    public Mensagem(string from, string body, long timestamp, string? notifyName)
    {
        From = from ?? throw new ArgumentNullException(nameof(from));
        Body = body ?? throw new ArgumentNullException(nameof(body));
        Timestamp = timestamp;
        NotifyName = notifyName;
        CreatedAt = DateTime.UtcNow;
    }
}
