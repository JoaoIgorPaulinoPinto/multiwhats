using System.Text.Json.Serialization;
using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.dtos.Requests;

public record UpdateUserRequest
{
    public string? Name { get; init; }
    public string? NewPassword { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UserRole? Role { get; init; }

    public bool? IsActive { get; init; }
}
