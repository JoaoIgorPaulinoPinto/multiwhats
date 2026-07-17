using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace multiwhats_api.src.data.entities;

[Table("Usuarios")]
public class Usuario : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; private set; }

    [Required(ErrorMessage = "O nome é obrigatório")]
    [MaxLength(200)]
    [Column("nome")]
    public string Nome { get; private set; } = null!;

    [Required(ErrorMessage = "O e-mail é obrigatório")]
    [MaxLength(200)]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    [Column("email")]
    public string Email { get; private set; } = null!;

    [Required(ErrorMessage = "A senha é obrigatória")]
    [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres")]
    [Column("senha")]
    public string Senha { get; private set; } = null!;

    [MaxLength(20)]
    [Column("telefone")]
    public string? Telefone { get; private set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Usuario() { }

    public Usuario(string nome, string email, string senha, string? telefone)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Senha = senha ?? throw new ArgumentNullException(nameof(senha));
        Telefone = telefone;
        CreatedAt = DateTime.UtcNow;
    }
}
