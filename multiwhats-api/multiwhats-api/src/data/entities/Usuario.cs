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

    [Required(ErrorMessage = "A senha é obrigatória")]
    [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres")]
    [Column("senha")]
    public string Senha { get; private set; } = null!;

    private Usuario() { }

    public Usuario(string nome, string senha)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Senha = senha ?? throw new ArgumentNullException(nameof(senha));
    }
}
