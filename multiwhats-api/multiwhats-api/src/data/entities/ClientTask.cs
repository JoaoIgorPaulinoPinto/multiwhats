using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.entities;

[Table("ClientTasks")]
public class ClientTask : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; private set; }

    [Required]
    [MaxLength(300)]
    [Column("title")]
    public string Title { get; private set; } = null!;

    [MaxLength(2000)]
    [Column("description")]
    public string? Description { get; private set; }

    [Column("status")]
    public ClientTaskStatus Status { get; private set; } = ClientTaskStatus.Open;

    [Column("priority")]
    public Priority Priority { get; private set; } = Priority.Medium;

    [Column("due_date")]
    public DateTime? DueDate { get; private set; }

    [Required]
    [Column("client_id")]
    public int ClientId { get; private set; }

    [ForeignKey(nameof(ClientId))]
    public Client Client { get; private set; } = null!;

    [Column("assigned_to_user_id")]
    public int? AssignedToUserId { get; private set; }

    [ForeignKey(nameof(AssignedToUserId))]
    public User? AssignedTo { get; private set; }

    [ForeignKey(nameof(CreatedByUserId))]
    public User? CreatedBy { get; private set; }

    private ClientTask() { }

    public ClientTask(
        string title,
        int clientId,
        int? createdByUserId = null,
        string? description = null,
        Priority priority = Priority.Medium,
        DateTime? dueDate = null,
        int? assignedToUserId = null)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        ClientId = clientId;
        CreatedByUserId = createdByUserId;
        Description = description;
        Priority = priority;
        DueDate = dueDate;
        AssignedToUserId = assignedToUserId;
    }

    public void Update(string? title, string? description, ClientTaskStatus? status, Priority? priority, DateTime? dueDate, int? assignedToUserId)
    {
        if (title != null) Title = title;
        if (description != null) Description = description;
        if (status.HasValue) Status = status.Value;
        if (priority.HasValue) Priority = priority.Value;
        if (dueDate.HasValue) DueDate = dueDate.Value;
        if (assignedToUserId.HasValue) AssignedToUserId = assignedToUserId;
    }

    public void UpdateStatus(ClientTaskStatus status)
    {
        Status = status;
    }
}
