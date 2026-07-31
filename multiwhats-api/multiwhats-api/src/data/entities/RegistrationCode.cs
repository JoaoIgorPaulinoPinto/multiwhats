using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace multiwhats_api.src.data.entities;

[Table("registration_codes")]
public class RegistrationCode
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; private set; }

    [Required]
    [MaxLength(EntityConstraints.RegistrationCodeValueMaxLength)]
    [Column("code")]
    public string Code { get; private set; } = null!;

    [Column("is_used")]
    public bool IsUsed { get; private set; }

    [Column("used_by_user_id")]
    public int? UsedByUserId { get; private set; }

    [Column("created_by_user_id")]
    public int CreatedByUserId { get; private set; }

    [Column("expires_at")]
    public DateTime ExpiresAt { get; private set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    [ForeignKey(nameof(CreatedByUserId))]
    public User CreatedBy { get; private set; } = null!;

    private RegistrationCode() { }

    public RegistrationCode(string code, int createdByUserId, int expiryHours)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
        CreatedByUserId = createdByUserId;
        ExpiresAt = DateTime.UtcNow.AddHours(expiryHours);
    }

    public bool IsValid()
    {
        return !IsUsed && ExpiresAt > DateTime.UtcNow;
    }

    public void MarkAsUsed(int userId)
    {
        IsUsed = true;
        UsedByUserId = userId;
    }
}
