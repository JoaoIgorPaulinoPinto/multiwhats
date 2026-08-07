namespace multiwhats_api.src.data.dtos.Responses;

// Full history of a chat: atendimento sessions, occurrences and a chronological timeline.
public record ChatHistoryResponse
{
    public int ChatId { get; init; }
    public List<ChatAtendimentoHistoryResponse> Atendimentos { get; init; } = new();
    public List<ChatOccurrenceHistoryResponse> Occurrences { get; init; } = new();
    public List<ChatHistoryTimelineItemResponse> Timeline { get; init; } = new();
}
