namespace multiwhats_api.src.data.dtos.Responses;

public record MensagemResponse
{
    public int Id { get; init; }
    public string From { get; init; } = null!;
    public string Body { get; init; } = null!;
    public long Timestamp { get; init; }
    public string? NotifyName { get; init; }
    public DateTime CreatedAt { get; init; }
    public int? ContatoId { get; init; }
}
