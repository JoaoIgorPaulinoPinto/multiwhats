using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace multiwhats_api.src.data.entities;

[Table("Contacts")]
public class Contact : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; private set; }

    [Required]
    [MaxLength(100)]
    [Column("jid")]
    public string Jid { get; private set; } = null!;

    [Required]
    [MaxLength(20)]
    [Column("phone_number")]
    public string PhoneNumber { get; private set; } = null!;

    [MaxLength(150)]
    [Column("name")]
    public string? Name { get; private set; }

    [MaxLength(150)]
    [Column("push_name")]
    public string? PushName { get; private set; }

    [MaxLength(500)]
    [Column("profile_pic_url")]
    public string? ProfilePicUrl { get; private set; }

    [Column("is_blocked")]
    public bool IsBlocked { get; private set; }

    [Column("is_group")]
    public bool IsGroup { get; private set; }

    [Column("last_message_at")]
    public DateTime? LastMessageAt { get; private set; }

    [Column("client_id")]
    public int? ClientId { get; private set; }

    [ForeignKey(nameof(ClientId))]
    public Client? Client { get; private set; }

    [Column("group_id")]
    public int? GroupId { get; private set; }

    [ForeignKey(nameof(GroupId))]
    public Group? Group { get; private set; }

    [ForeignKey(nameof(CreatedByUserId))]
    public User? CreatedBy { get; private set; }

    public Chat? Chat { get; private set; }

    private Contact() { }

    public Contact(
        string jid,
        string phoneNumber,
        string? name,
        string? pushName,
        int? createdByUserId = null,
        int? clientId = null,
        int? groupId = null)
    {
        Jid = jid ?? throw new ArgumentNullException(nameof(jid));
        PhoneNumber = phoneNumber ?? throw new ArgumentNullException(nameof(phoneNumber));
        Name = name;
        PushName = pushName;
        ClientId = clientId;
        GroupId = groupId;
        CreatedByUserId = createdByUserId;
    }

    public void AssignToClient(int clientId)
    {
        ClientId = clientId;
    }

    public void UnassignFromClient()
    {
        ClientId = null;
    }

    public void UpdateInfo(string? name, string? pushName, string? profilePicUrl, bool? isBlocked)
    {
        if (name != null) Name = name;
        if (pushName != null) PushName = pushName;
        if (profilePicUrl != null) ProfilePicUrl = profilePicUrl;
        if (isBlocked.HasValue) IsBlocked = isBlocked.Value;
    }

    public void UpdateLastMessageAt(DateTime timestamp)
    {
        LastMessageAt = timestamp;
    }
}
