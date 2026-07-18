using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace multiwhats_api.src.data.entities;

[Table("Groups")]
public class Group : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; private set; }

    [Required]
    [MaxLength(200)]
    [Column("name")]
    public string Name { get; private set; } = null!;

    [MaxLength(500)]
    [Column("description")]
    public string? Description { get; private set; }

    [MaxLength(100)]
    [Column("whatsapp_group_id")]
    public string? WhatsAppGroupId { get; private set; }

    public ICollection<Contact> Members { get; private set; } = new List<Contact>();

    private Group() { }

    public Group(string name, string? description = null, string? whatsAppGroupId = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        WhatsAppGroupId = whatsAppGroupId;
    }

    public void Update(string? name, string? description, string? whatsAppGroupId)
    {
        if (name != null) Name = name;
        if (description != null) Description = description;
        if (whatsAppGroupId != null) WhatsAppGroupId = whatsAppGroupId;
    }
}
