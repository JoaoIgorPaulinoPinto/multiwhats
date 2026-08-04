using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.OccurrenceInterfaces;

namespace multiwhats_api.src.usecases.usecases.OccurrenceUseCases;

public class UpdateOccurrenceUseCase : IUpdateOccurrenceUseCase
{
    private readonly IOccurrenceRepository _occurrenceRepository;
    private readonly UseCaseLogger _useCaseLogger;

    public UpdateOccurrenceUseCase(IOccurrenceRepository occurrenceRepository, UseCaseLogger useCaseLogger)
    {
        _occurrenceRepository = occurrenceRepository;
        _useCaseLogger = useCaseLogger;
    }

    public async Task<OccurrenceDetailResponse> Execute(int id, UpdateOccurrenceRequest request)
    {
        var occurrence = await _occurrenceRepository.GetByIdAsync(id);
        if (occurrence == null)
            throw new KeyNotFoundException("Ocorrência não encontrada");

        occurrence.Update(request.Title, request.Description, request.Status, request.Priority, request.AssignedToUserId);
        await _occurrenceRepository.UpdateAsync(occurrence);

        var updated = await _occurrenceRepository.GetByIdAsync(id);
        if (updated == null)
            throw new KeyNotFoundException("Ocorrência não encontrada após atualização");

        await _useCaseLogger.LogAsync(
            action: "UpdateOccurrence",
            entityType: "Occurrence",
            entityId: id,
            description: $"Updated occurrence #{id} (Title: \"{updated.Title}\", Status: {updated.Status}, Priority: {updated.Priority})"
        );

        return new OccurrenceDetailResponse
        {
            Id = updated.Id,
            Title = updated.Title,
            Description = updated.Description,
            Status = updated.Status,
            Priority = updated.Priority,
            ChatName = updated.Chat?.Name ?? updated.Chat?.PhoneNumber,
            AssignedToUserId = updated.AssignedToUserId,
            AssignedToName = updated.AssignedTo?.Name,
            CreatedByName = updated.CreatedBy?.Name,
            MessageCount = updated.Messages?.Count ?? 0,
            CreatedAt = updated.CreatedAt,
            LastUpdate = updated.LastUpdate
        };
    }
}
