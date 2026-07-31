using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.OccurrenceInterfaces;

namespace multiwhats_api.src.usecases.usecases.OccurrenceUseCases;

public class GetOccurrenceMetricsUseCase : IGetOccurrenceMetricsUseCase
{
    private readonly IOccurrenceRepository _occurrenceRepository;
    private readonly UseCaseLogger _useCaseLogger;

    public GetOccurrenceMetricsUseCase(IOccurrenceRepository occurrenceRepository, UseCaseLogger useCaseLogger)
    {
        _occurrenceRepository = occurrenceRepository;
        _useCaseLogger = useCaseLogger;
    }

    public async Task<OccurrenceMetricsResponse> Execute()
    {
        var all = await _occurrenceRepository.GetAllAsync();
        var closed = all.Where(o => o.Status == data.enums.OccurrenceStatus.Closed).ToList();

        var totalClosed = closed.Count;

        var avgHours = totalClosed > 0
            ? closed.Average(o => (o.LastUpdate - o.CreatedAt).TotalHours)
            : 0;

        var perDay = closed
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new DailyCount
            {
                Date = g.Key.ToString("yyyy-MM-dd"),
                Count = g.Count()
            })
            .OrderBy(d => d.Date)
            .ToList();

        var allUsers = all
            .GroupBy(o => o.CreatedByUserId)
            .Select(g =>
            {
                var user = g.First().CreatedBy;
                return new UserMetrics
                {
                    UserId = g.Key ?? 0,
                    UserName = user?.Name,
                    Opened = g.Count(),
                    Closed = g.Count(o => o.Status == data.enums.OccurrenceStatus.Closed)
                };
            })
            .OrderByDescending(u => u.Opened)
            .ToList();

        await _useCaseLogger.LogAsync(
            action: "GetOccurrenceMetrics",
            entityType: "Occurrence",
            entityId: null,
            description: $"Computed metrics for {totalClosed} closed occurrences"
        );

        return new OccurrenceMetricsResponse
        {
            AverageResolutionHours = Math.Round(avgHours, 1),
            OccurrencesPerDay = perDay,
            PerUser = allUsers,
            TotalClosed = totalClosed
        };
    }
}
