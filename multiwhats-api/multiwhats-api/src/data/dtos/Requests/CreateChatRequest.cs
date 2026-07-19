namespace multiwhats_api.src.data.dtos.Requests;

public record CreateChatRequest
{
    public string Jid { get; init; } = null!;
    public string PhoneNumber { get; init; } = null!;
    public string? Name { get; init; }
    public int? ContactId { get; init; }
    public int? ClientId { get; init; }
}

