namespace multiwhats_api.src.data.dtos.Responses;

public record ChatAssignedResponse
{
    public int Id { get; init; }
    public int? AssignedToUserId { get; init; }
    public string? AssignedToUserName { get; init; }
}
