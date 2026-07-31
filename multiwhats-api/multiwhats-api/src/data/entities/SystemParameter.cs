using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace multiwhats_api.src.data.entities;

[Table("system_parameters")]
public class SystemParameter
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; private set; }

    [Required]
    [MaxLength(100)]
    [Column("key")]
    public string Key { get; private set; } = null!;

    [Column("value")]
    public string? Value { get; private set; }

    [Required]
    [MaxLength(20)]
    [Column("type")]
    public string Type { get; private set; } = "String";

    [MaxLength(50)]
    [Column("group")]
    public string? Group { get; private set; }

    [MaxLength(500)]
    [Column("description")]
    public string? Description { get; private set; }

    [Column("is_required")]
    public bool IsRequired { get; private set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    [Column("updated_by_user_id")]
    public int? UpdatedByUserId { get; private set; }

    private SystemParameter() { }

    public SystemParameter(string key, string? value, string type, string? group = null, string? description = null, bool isRequired = false)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
        Value = value;
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Group = group;
        Description = description;
        IsRequired = isRequired;
    }

    public void UpdateValue(string? value, int? userId)
    {
        Value = value;
        UpdatedAt = DateTime.UtcNow;
        UpdatedByUserId = userId;
    }
}
