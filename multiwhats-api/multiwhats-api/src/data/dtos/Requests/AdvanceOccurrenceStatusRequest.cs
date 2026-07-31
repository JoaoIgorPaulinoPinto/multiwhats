using System.ComponentModel.DataAnnotations;

namespace multiwhats_api.src.data.dtos.Requests;

public record AdvanceOccurrenceStatusRequest
{
    [Required]
    public AdvanceDirection Direction { get; init; }
}

public enum AdvanceDirection
{
    Advance,
    Return
}
