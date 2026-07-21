namespace multiwhats_api.src.data.dtos.Requests;

public record DeviceRequest
{
    public string Jid { get; init; } = null!;
    public string? PhoneNumber { get; init; }
    public string? PushName { get; init; }
    public string? Platform { get; init; }
}
