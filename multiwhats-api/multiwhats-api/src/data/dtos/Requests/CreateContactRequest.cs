using System.ComponentModel.DataAnnotations;

namespace multiwhats_api.src.data.dtos.Requests;

public record CreateContactRequest
{
    [Required(ErrorMessage = "O JID é obrigatório")]
    [MaxLength(100)]
    public string Jid { get; init; } = null!;

    [Required(ErrorMessage = "O número é obrigatório")]
    [MaxLength(20)]
    public string PhoneNumber { get; init; } = null!;

    [MaxLength(150)]
    public string? Name { get; init; }

    [MaxLength(150)]
    public string? PushName { get; init; }

    public int? ClientId { get; init; }

    public int? GroupId { get; init; }
}
