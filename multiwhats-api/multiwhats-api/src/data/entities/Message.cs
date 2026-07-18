using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.entities;

[Table("Messages")]
public class Message : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; private set; }

    [MaxLength(200)]
    [Column("message_id")]
    public string? MessageId { get; private set; }

    [Required]
    [MaxLength(100)]
    [Column("from_jid")]
    public string FromJid { get; private set; } = null!;

    [MaxLength(100)]
    [Column("to_jid")]
    public string? ToJid { get; private set; }

    [Required]
    [MaxLength(20)]
    [Column("phone_number")]
    public string PhoneNumber { get; private set; } = null!;

    [Column("body")]
    public string? Body { get; private set; }

    [Column("direction")]
    public MessageDirection Direction { get; private set; }

    [Column("type")]
    public MessageType Type { get; private set; }

    [Column("timestamp")]
    public long Timestamp { get; private set; }

    [Column("sent_at")]
    public DateTime SentAt { get; private set; }

    [MaxLength(100)]
    [Column("notify_name")]
    public string? NotifyName { get; private set; }

    [Column("has_media")]
    public bool HasMedia { get; private set; }

    [MaxLength(1000)]
    [Column("media_url")]
    public string? MediaUrl { get; private set; }

    [MaxLength(100)]
    [Column("media_mime_type")]
    public string? MediaMimeType { get; private set; }

    [MaxLength(255)]
    [Column("media_filename")]
    public string? MediaFilename { get; private set; }

    [Column("media_size")]
    public long? MediaSize { get; private set; }

    [MaxLength(500)]
    [Column("media_caption")]
    public string? MediaCaption { get; private set; }

    [Column("delivery_status")]
    public DeliveryStatus DeliveryStatus { get; private set; }

    [Column("is_forwarded")]
    public bool IsForwarded { get; private set; }

    [Column("contact_id")]
    public int? ContactId { get; private set; }

    [ForeignKey(nameof(ContactId))]
    public Contact? Contact { get; private set; }

    [Column("user_id")]
    public int? UserId { get; private set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; private set; }

    [Column("occurrence_id")]
    public int? OccurrenceId { get; private set; }

    [ForeignKey(nameof(OccurrenceId))]
    public Occurrence? Occurrence { get; private set; }

    [Column("reply_to_id")]
    public int? ReplyToId { get; private set; }

    [ForeignKey(nameof(ReplyToId))]
    public Message? ReplyTo { get; private set; }

    private Message() { }

    public Message(
        string fromJid,
        string phoneNumber,
        string? body,
        MessageDirection direction,
        MessageType type,
        long timestamp,
        int? contactId = null,
        int? userId = null,
        int? occurrenceId = null,
        string? toJid = null,
        string? messageId = null,
        string? notifyName = null,
        bool hasMedia = false,
        string? mediaUrl = null,
        string? mediaMimeType = null,
        string? mediaFilename = null,
        long? mediaSize = null,
        string? mediaCaption = null,
        bool isForwarded = false,
        int? replyToId = null)
    {
        FromJid = fromJid ?? throw new ArgumentNullException(nameof(fromJid));
        PhoneNumber = phoneNumber ?? throw new ArgumentNullException(nameof(phoneNumber));
        Body = body;
        Direction = direction;
        Type = type;
        Timestamp = timestamp;
        SentAt = DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;
        ContactId = contactId;
        UserId = userId;
        OccurrenceId = occurrenceId;
        ToJid = toJid;
        MessageId = messageId;
        NotifyName = notifyName;
        HasMedia = hasMedia;
        MediaUrl = mediaUrl;
        MediaMimeType = mediaMimeType;
        MediaFilename = mediaFilename;
        MediaSize = mediaSize;
        MediaCaption = mediaCaption;
        IsForwarded = isForwarded;
        ReplyToId = replyToId;
        DeliveryStatus = direction == MessageDirection.Outgoing ? DeliveryStatus.Pending : DeliveryStatus.Delivered;
    }

    public void UpdateDeliveryStatus(DeliveryStatus status)
    {
        DeliveryStatus = status;
    }

    public void LinkToOccurrence(int occurrenceId)
    {
        OccurrenceId = occurrenceId;
    }
}
