using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.entities;

[Table("Users")]
public class User : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; private set; }

    [Required]
    [MaxLength(200)]
    [Column("name")]
    public string Name { get; private set; } = null!;

    [Required]
    [MinLength(6)]
    [Column("password")]
    public string Password { get; private set; } = null!;

    [Column("role")]
    public UserRole Role { get; private set; } = UserRole.Support;

    [Column("is_active")]
    public bool IsActive { get; private set; } = true;

    private User() { }

    public User(string name, string password, UserRole role = UserRole.Support)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Password = password ?? throw new ArgumentNullException(nameof(password));
        Role = role;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void ChangePassword(string newPassword)
    {
        Password = newPassword ?? throw new ArgumentNullException(nameof(newPassword));
    }

    public void ChangeRole(UserRole newRole)
    {
        Role = newRole;
    }
}
