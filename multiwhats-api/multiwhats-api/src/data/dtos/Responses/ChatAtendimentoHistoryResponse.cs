namespace multiwhats_api.src.data.dtos.Responses;

// A single atendimento session (assign -> unassign) of a chat by one or more users.
public record ChatAtendimentoHistoryResponse
{
    public int Id { get; init; }
    public DateTime StartedAt { get; init; }
    public DateTime? EndedAt { get; init; }
    public bool IsOpen { get; init; }
    public int? StartedByUserId { get; init; }
    public string? StartedByName { get; init; }
    public int? EndedByUserId { get; init; }
    public string? EndedByName { get; init; }
    public long? DurationSeconds { get; init; }
}
