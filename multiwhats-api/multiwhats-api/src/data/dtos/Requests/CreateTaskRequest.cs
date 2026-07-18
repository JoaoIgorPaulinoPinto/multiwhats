using System.ComponentModel.DataAnnotations;
using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.dtos.Requests;

public record CreateTaskRequest
{
    [Required(ErrorMessage = "O título é obrigatório")]
    [MaxLength(300)]
    public string Title { get; init; } = null!;

    [MaxLength(2000)]
    public string? Description { get; init; }

    public Priority Priority { get; init; } = Priority.Medium;

    public DateTime? DueDate { get; init; }

    [Required]
    public int ClientId { get; init; }
}
