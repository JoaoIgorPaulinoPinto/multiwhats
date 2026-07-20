using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace multiwhats_api.src.data.entities;

[Table("Chats")]
public class Chat : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; private set; }

    [Required]
    [MaxLength(100)]
    [Column("jid")]
    public string Jid { get; private set; } = null!;

    [MaxLength(20)]
    [Column("phone_number")]
    public string? PhoneNumber { get; private set; }

    [MaxLength(150)]
    [Column("name")]
    public string? Name { get; private set; }

    [Column("contact_id")]
    public int? ContactId { get; private set; }

    [ForeignKey(nameof(ContactId))]
    public Contact? Contact { get; private set; }

    [Column("client_id")]
    public int? ClientId { get; private set; }

    [ForeignKey(nameof(ClientId))]
    public Client? Client { get; private set; }

    [Column("last_message_at")]
    public DateTime? LastMessageAt { get; private set; }

    [Column("last_message_body")]
    public string? LastMessageBody { get; private set; }

    [Column("assigned_to_user_id")]
    public int? AssignedToUserId { get; private set; }

    [ForeignKey(nameof(AssignedToUserId))]
    public User? AssignedTo { get; private set; }

    [ForeignKey(nameof(CreatedByUserId))]
    public User? CreatedBy { get; private set; }

    public ICollection<Message> Messages { get; private set; } = new List<Message>();
    public ICollection<Occurrence> Occurrences { get; private set; } = new List<Occurrence>();

    private Chat() { }

    public Chat(
        string jid,
        string? phoneNumber = null,
        string? name = null,
        int? contactId = null,
        int? clientId = null,
        int? assignedToUserId = null)
    {
        Jid = jid ?? throw new ArgumentNullException(nameof(jid));
        PhoneNumber = phoneNumber;
        Name = name;
        ContactId = contactId;
        ClientId = clientId;
        AssignedToUserId = assignedToUserId;
    }

    public void LinkToContact(int contactId, int? clientId = null)
    {
        ContactId = contactId;
        if (clientId.HasValue)
            ClientId = clientId;
    }

    public void UnlinkContact()
    {
        ContactId = null;
    }

    public void UpdateLastMessage(DateTime timestamp, string? body)
    {
        LastMessageAt = timestamp;
        LastMessageBody = body;
    }

    public void AssignUser(int userId)
    {
        AssignedToUserId = userId;
    }

    public void UpdateName(string? name)
    {
        if (name != null) Name = name;
    }
}
