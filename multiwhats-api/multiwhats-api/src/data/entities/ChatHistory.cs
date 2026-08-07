using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace multiwhats_api.src.data.entities;

[Table("chat_history")]
public class ChatHistory
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; private set; }

    [Required]
    [Column("chat_id")]
    public int ChatId { get; private set; }

    [Required]
    [Column("assigned_to_user_id")]
    public int AssignedToUserId { get; private set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    [Column("unassigned_at")]
    public DateTime? UnassignedAt { get; private set; }

    [ForeignKey(nameof(ChatId))]
    public Chat? Chat { get; private set; }

    [ForeignKey(nameof(AssignedToUserId))]
    public User? AssignedToUser { get; private set; }

    public ChatHistory(int chatId, int assignedToUserId)
    {
        ChatId = chatId;
        AssignedToUserId = assignedToUserId;
    }

    public void Finalize()
    {
        UnassignedAt = DateTime.UtcNow;
    }
}
