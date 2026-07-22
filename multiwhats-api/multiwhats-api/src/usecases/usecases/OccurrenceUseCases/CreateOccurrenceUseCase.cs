using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.OccurrenceInterfaces;

namespace multiwhats_api.src.usecases.usecases.OccurrenceUseCases;

public class CreateOccurrenceUseCase : ICreateOccurrenceUseCase
{
    private readonly IOccurrenceRepository _occurrenceRepository;
    private readonly UseCaseLogger _useCaseLogger;

    public CreateOccurrenceUseCase(IOccurrenceRepository occurrenceRepository, UseCaseLogger useCaseLogger)
    {
        _occurrenceRepository = occurrenceRepository;
        _useCaseLogger = useCaseLogger;
    }

    public async Task<OccurrenceDetailResponse> Execute(CreateOccurrenceRequest request, int userId)
    {
        var occurrence = new Occurrence(
            request.Title,
            request.ChatId,
            userId,
            request.Description,
            request.Priority
        );

        var created = await _occurrenceRepository.AddAsync(occurrence);

        await _useCaseLogger.LogAsync(
            action: "CreateOccurrence",
            entityType: "Occurrence",
            entityId: created.Id,
            description: $"Created occurrence \"{created.Title}\" (ChatId: {created.ChatId}, Priority: {created.Priority}, Status: {created.Status})",
            explicitUserId: userId
        );

        return new OccurrenceDetailResponse
        {
            Id = created.Id,
            Title = created.Title,
            Description = created.Description,
            Status = created.Status,
            Priority = created.Priority,
            ChatId = created.ChatId,
            AssignedToUserId = created.AssignedToUserId,
            CreatedByUserId = created.CreatedByUserId,
            CreatedAt = created.CreatedAt,
            LastUpdate = created.LastUpdate
        };
    }
}
