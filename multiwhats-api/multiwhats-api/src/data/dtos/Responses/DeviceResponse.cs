namespace multiwhats_api.src.data.dtos.Responses;

public record DeviceResponse
{
    public int Id { get; init; }
    public string Jid { get; init; } = null!;
    public string PhoneNumber { get; init; } = null!;
    public string? PushName { get; init; }
    public string? Platform { get; init; }
    public DateTime ConnectedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
