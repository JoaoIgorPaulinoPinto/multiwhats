using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.entities;

[Table("Clients")]
public class Client : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; private set; }

    [Required]
    [MaxLength(200)]
    [Column("name")]
    public string Name { get; private set; } = null!;

    [MaxLength(20)]
    [Column("main_phone_number")]
    public string? MainPhoneNumber { get; private set; }

    [Column("status")]
    public ClientStatus Status { get; private set; } = ClientStatus.Active;

    public ICollection<Contact> Contacts { get; private set; } = new List<Contact>();
    public ICollection<Chat> Chats { get; private set; } = new List<Chat>();
    public ICollection<ClientTask> Tasks { get; private set; } = new List<ClientTask>();

    private Client() { }

    public Client(string name, string? mainPhoneNumber = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        MainPhoneNumber = mainPhoneNumber;
    }

    public void Update(string? name, string? mainPhoneNumber, ClientStatus? status)
    {
        if (name != null) Name = name;
        if (mainPhoneNumber != null) MainPhoneNumber = mainPhoneNumber;
        if (status.HasValue) Status = status.Value;
    }
}
