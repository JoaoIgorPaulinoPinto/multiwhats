using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.data.enums;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.OccurrenceInterfaces;

namespace multiwhats_api.src.usecases.usecases.OccurrenceUseCases;

public class AdvanceOccurrenceStatusUseCase : IAdvanceOccurrenceStatusUseCase
{
    private readonly IOccurrenceRepository _occurrenceRepository;
    private readonly UseCaseLogger _useCaseLogger;
    private readonly SystemConfigService _config;

    public AdvanceOccurrenceStatusUseCase(
        IOccurrenceRepository occurrenceRepository,
        UseCaseLogger useCaseLogger,
        SystemConfigService config)
    {
        _occurrenceRepository = occurrenceRepository;
        _useCaseLogger = useCaseLogger;
        _config = config;
    }

    public async Task<OccurrenceDetailResponse> Execute(int id, AdvanceOccurrenceStatusRequest request, int userId)
    {
        var occurrence = await _occurrenceRepository.GetByIdAsync(id);
        if (occurrence == null)
            throw new KeyNotFoundException("Ocorrência não encontrada");

        var defaultFlow = new List<string> { "Open", "InProgress", "Resolved", "Closed" };
        var statusFlow = (await _config.GetListAsync("Occurrence:StatusFlow", defaultFlow))
            .Select(s => Enum.Parse<OccurrenceStatus>(s))
            .ToList();

        var direction = request.Direction == AdvanceDirection.Advance ? "avançada" : "retrocedida";
        occurrence.AdvanceStatus(request.Direction == AdvanceDirection.Advance, statusFlow);
        await _occurrenceRepository.UpdateAsync(occurrence);

        var updated = await _occurrenceRepository.GetByIdAsync(id);
        if (updated == null)
            throw new KeyNotFoundException("Ocorrência não encontrada após atualização");

        await _useCaseLogger.LogAsync(
            action: "AdvanceOccurrenceStatus",
            entityType: "Occurrence",
            entityId: id,
            description: $"Ocorrência #{id} {direction} para status {updated.Status} (usuário #{userId})"
        );
        var contactName = occurrence.Chat.Contact?.Name ?? updated.Chat?.Name ?? updated.Chat?.PhoneNumber;
        return new OccurrenceDetailResponse
        {
            Id = updated.Id,
            Title = updated.Title,
            Description = updated.Description,
            Status = updated.Status,
            Priority = updated.Priority,
            ChatName = contactName,
            AssignedToUserId = updated.AssignedToUserId,
            AssignedToName = updated.AssignedTo?.Name,
            CreatedByName = updated.CreatedBy?.Name,
            MessageCount = updated.Messages?.Count ?? 0,
            CreatedAt = updated.CreatedAt,
            LastUpdate = updated.LastUpdate
        };
    }
}
