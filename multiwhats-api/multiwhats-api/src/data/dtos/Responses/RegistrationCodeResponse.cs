namespace multiwhats_api.src.data.dtos.Responses;

public record RegistrationCodeResponse
{
    public int Id { get; init; }
    public string Code { get; init; } = null!;
    public bool IsUsed { get; init; }
    public int? UsedByUserId { get; init; }
    public DateTime ExpiresAt { get; init; }
    public DateTime CreatedAt { get; init; }
}
