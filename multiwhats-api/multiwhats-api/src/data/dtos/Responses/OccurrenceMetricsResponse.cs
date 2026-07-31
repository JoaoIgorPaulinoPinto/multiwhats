namespace multiwhats_api.src.data.dtos.Responses;

public record OccurrenceMetricsResponse
{
    public double AverageResolutionHours { get; init; }
    public List<DailyCount> OccurrencesPerDay { get; init; } = [];
    public List<UserMetrics> PerUser { get; init; } = [];
    public int TotalClosed { get; init; }
}

public record DailyCount
{
    public string Date { get; init; } = null!;
    public int Count { get; init; }
}

public record UserMetrics
{
    public int UserId { get; init; }
    public string? UserName { get; init; }
    public int Opened { get; init; }
    public int Closed { get; init; }
}
