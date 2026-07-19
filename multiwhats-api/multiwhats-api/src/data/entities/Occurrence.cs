using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.entities;

[Table("Occurrences")]
public class Occurrence : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; private set; }

    [Required]
    [MaxLength(200)]
    [Column("title")]
    public string Title { get; private set; } = null!;

    [MaxLength(2000)]
    [Column("description")]
    public string? Description { get; private set; }

    [Column("status")]
    public OccurrenceStatus Status { get; private set; } = OccurrenceStatus.Open;

    [Column("priority")]
    public Priority Priority { get; private set; } = Priority.Medium;

    [Required]
    [Column("chat_id")]
    public int ChatId { get; private set; }

    [ForeignKey(nameof(ChatId))]
    public Chat Chat { get; private set; } = null!;

    [Column("assigned_to_user_id")]
    public int? AssignedToUserId { get; private set; }

    [ForeignKey(nameof(AssignedToUserId))]
    public User? AssignedTo { get; private set; }

    [ForeignKey(nameof(CreatedByUserId))]
    public User? CreatedBy { get; private set; }

    public ICollection<Message> Messages { get; private set; } = new List<Message>();

    private Occurrence() { }

    public Occurrence(
        string title,
        int chatId,
        int? createdByUserId = null,
        string? description = null,
        Priority priority = Priority.Medium,
        int? assignedToUserId = null)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        ChatId = chatId;
        CreatedByUserId = createdByUserId;
        Description = description;
        Priority = priority;
        AssignedToUserId = assignedToUserId;
    }

    public void Update(string? title, string? description, OccurrenceStatus? status, Priority? priority, int? assignedToUserId)
    {
        if (title != null) Title = title;
        if (description != null) Description = description;
        if (status.HasValue) Status = status.Value;
        if (priority.HasValue) Priority = priority.Value;
        if (assignedToUserId.HasValue) AssignedToUserId = assignedToUserId;
    }
}
