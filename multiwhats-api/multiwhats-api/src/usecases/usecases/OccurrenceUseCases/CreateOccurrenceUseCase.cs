using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.usecases.interfaces.OccurrenceInterfaces;

namespace multiwhats_api.src.usecases.usecases.OccurrenceUseCases;

public class CreateOccurrenceUseCase : ICreateOccurrenceUseCase
{
    private readonly IOccurrenceRepository _occurrenceRepository;

    public CreateOccurrenceUseCase(IOccurrenceRepository occurrenceRepository)
    {
        _occurrenceRepository = occurrenceRepository;
    }

    public async Task<OccurrenceResponse> Execute(CreateOccurrenceRequest request, int userId)
    {
        var occurrence = new Occurrence(
            request.Title,
            request.ContactId,
            userId,
            request.Description,
            request.Priority
        );

        var created = await _occurrenceRepository.AddAsync(occurrence);

        return new OccurrenceResponse
        {
            Id = created.Id,
            Title = created.Title,
            Description = created.Description,
            Status = created.Status,
            Priority = created.Priority,
            ContactId = created.ContactId,
            AssignedToUserId = created.AssignedToUserId,
            CreatedByUserId = created.CreatedByUserId,
            CreatedAt = created.CreatedAt,
            LastUpdate = created.LastUpdate
        };
    }
}
