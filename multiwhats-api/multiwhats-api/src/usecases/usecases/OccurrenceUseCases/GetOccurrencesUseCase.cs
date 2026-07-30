using multiwhats_api.src.data.entities;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.OccurrenceInterfaces;

namespace multiwhats_api.src.usecases.usecases.OccurrenceUseCases;

public class GetOccurrencesUseCase : IGetOccurrencesUseCase
{
    private readonly IOccurrenceRepository _occurrenceRepository;
    private readonly UseCaseLogger _useCaseLogger;

    public GetOccurrencesUseCase(IOccurrenceRepository occurrenceRepository, UseCaseLogger useCaseLogger)
    {
        _occurrenceRepository = occurrenceRepository;
        _useCaseLogger = useCaseLogger;
    }

    public async Task<List<OccurrenceListResponse>> ExecuteAll()
    {
        var occurrences = await _occurrenceRepository.GetAllAsync();

        await _useCaseLogger.LogAsync(
            action: "GetOccurrences",
            entityType: "Occurrence",
            entityId: null,
            description: $"Listed all occurrences (count: {occurrences.Count})"
        );

        return MapToListResponses(occurrences);
    }

    public async Task<OccurrenceDetailResponse?> ExecuteById(int id)
    {
        var occurrence = await _occurrenceRepository.GetByIdAsync(id);

        await _useCaseLogger.LogAsync(
            action: "GetOccurrence",
            entityType: "Occurrence",
            entityId: id,
            description: occurrence != null
                ? $"Retrieved occurrence #{id} (Title: {occurrence.Title}, Status: {occurrence.Status})"
                : $"Occurrence #{id} not found"
        );

        return occurrence != null ? MapToDetailResponse(occurrence) : null;
    }

    public async Task<List<ChatOccurrenceSummaryResponse>> ExecuteByChat(int chatId)
    {
        var occurrences = await _occurrenceRepository.GetByChatAsync(chatId);

        await _useCaseLogger.LogAsync(
            action: "GetOccurrences",
            entityType: "Occurrence",
            entityId: null,
            description: $"Listed occurrences for chat #{chatId} (count: {occurrences.Count})"
        );

        return occurrences.Select(MapToChatSummaryResponse).ToList();
    }

    private static OccurrenceListResponse MapToListResponse(Occurrence o) => new()
    {
        Id = o.Id,
        Title = o.Title,
        Status = o.Status,
        Priority = o.Priority,
        ChatName = o.Chat?.Name ?? o.Chat?.PhoneNumber,
        AssignedToName = o.AssignedTo?.Name,
        MessageCount = o.Messages.Count,
        CreatedAt = o.CreatedAt
    };

    private static OccurrenceDetailResponse MapToDetailResponse(Occurrence o) => new()
    {
        Id = o.Id,
        Title = o.Title,
        Description = o.Description,
        Status = o.Status,
        Priority = o.Priority,
        ChatId = o.ChatId,
        ChatName = o.Chat?.Name ?? o.Chat?.PhoneNumber,
        AssignedToUserId = o.AssignedToUserId,
        AssignedToName = o.AssignedTo?.Name,
        CreatedByUserId = o.CreatedByUserId,
        CreatedByName = o.CreatedBy?.Name,
        MessageCount = o.Messages.Count,
        CreatedAt = o.CreatedAt,
        LastUpdate = o.LastUpdate
    };

    private static ChatOccurrenceSummaryResponse MapToChatSummaryResponse(Occurrence o) => new()
    {
        Id = o.Id,
        Title = o.Title,
        Status = o.Status,
        Priority = o.Priority,
        AssignedToName = o.AssignedTo?.Name,
        MessageCount = o.Messages.Count,
        CreatedAt = o.CreatedAt
    };

    private static List<OccurrenceListResponse> MapToListResponses(List<Occurrence> occurrences) =>
        occurrences.Select(MapToListResponse).ToList();
}
