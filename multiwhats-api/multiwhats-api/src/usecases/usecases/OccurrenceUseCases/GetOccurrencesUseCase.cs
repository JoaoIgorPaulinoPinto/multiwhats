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

    public async Task<List<OccurrenceResponse>> ExecuteAll()
    {
        var occurrences = await _occurrenceRepository.GetAllAsync();

        await _useCaseLogger.LogAsync(
            action: "GetOccurrences",
            entityType: "Occurrence",
            entityId: null,
            description: $"Listed all occurrences (count: {occurrences.Count})"
        );

        return MapResponses(occurrences);
    }

    public async Task<OccurrenceResponse?> ExecuteById(int id)
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

        return occurrence != null ? MapResponse(occurrence) : null;
    }

    public async Task<List<OccurrenceResponse>> ExecuteByChat(int chatId)
    {
        var occurrences = await _occurrenceRepository.GetByChatAsync(chatId);

        await _useCaseLogger.LogAsync(
            action: "GetOccurrences",
            entityType: "Occurrence",
            entityId: null,
            description: $"Listed occurrences for chat #{chatId} (count: {occurrences.Count})"
        );

        return MapResponses(occurrences);
    }

    private static OccurrenceResponse MapResponse(Occurrence o) => new()
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

    private static List<OccurrenceResponse> MapResponses(List<Occurrence> occurrences) =>
        occurrences.Select(MapResponse).ToList();
}
