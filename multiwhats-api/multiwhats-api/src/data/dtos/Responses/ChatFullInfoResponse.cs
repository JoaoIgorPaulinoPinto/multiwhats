using multiwhats_api.src.data.enums;

namespace multiwhats_api.src.data.dtos.Responses;

public record ChatFullInfoResponse
{
    public int Id { get; init; }
    public string Jid { get; init; } = null!;
    public string? PhoneNumber { get; init; }
    public string? Name { get; init; }

    public int? ContactId { get; init; }
    public string? ContactName { get; init; }
    public string? ContactPushName { get; init; }
    public string? ContactProfilePicUrl { get; init; }
    public bool ContactIsBlocked { get; init; }
    public bool ContactIsGroup { get; init; }

    public int? ClientId { get; init; }
    public string? ClientName { get; init; }
    public string? ClientMainPhoneNumber { get; init; }

    public int? AssignedToUserId { get; init; }
    public string? AssignedToUserName { get; init; }
    public int? CreatedByUserId { get; init; }
    public string? CreatedByName { get; init; }

    public DateTime? LastMessageAt { get; init; }
    public LastMessageResponse? LastMessage { get; init; }

    public int MessageCount { get; init; }
    public int OutgoingMessageCount { get; init; }
    public int IncomingMessageCount { get; init; }
    public int ImageCount { get; init; }
    public int VideoCount { get; init; }
    public int AudioCount { get; init; }
    public int DocumentCount { get; init; }
    public int TextCount { get; init; }
    public int StickerCount { get; init; }
    public int MediaCount { get; init; }
    public int MediaSentCount { get; init; }
    public int DaysActive { get; init; }
    public long? TimeSinceLastOccurrenceSeconds { get; init; }

    public List<ChatOccurrenceSummaryResponse>? Occurrences { get; init; }
    public int OccurrenceCount { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime LastUpdate { get; init; }
}
