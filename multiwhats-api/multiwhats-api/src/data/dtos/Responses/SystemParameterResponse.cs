namespace multiwhats_api.src.data.dtos.Responses;

public record SystemParameterResponse
{
    public int Id { get; init; }
    public string Key { get; init; } = null!;
    public string? Value { get; init; }
    public string Type { get; init; } = null!;
    public string? Group { get; init; }
    public string? Description { get; init; }
    public bool IsRequired { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public int? UpdatedByUserId { get; init; }
}
