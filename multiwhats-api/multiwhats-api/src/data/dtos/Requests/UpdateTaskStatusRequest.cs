using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.dtos.Requests;

public record UpdateTaskStatusRequest
{
    public ClientTaskStatus Status { get; init; }
}
