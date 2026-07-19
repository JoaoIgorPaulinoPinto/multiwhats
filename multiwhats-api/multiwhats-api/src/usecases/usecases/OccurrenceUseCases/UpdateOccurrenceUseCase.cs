using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.usecases.interfaces.OccurrenceInterfaces;

namespace multiwhats_api.src.usecases.usecases.OccurrenceUseCases;

public class UpdateOccurrenceUseCase : IUpdateOccurrenceUseCase
{
    private readonly IOccurrenceRepository _occurrenceRepository;

    public UpdateOccurrenceUseCase(IOccurrenceRepository occurrenceRepository)
    {
        _occurrenceRepository = occurrenceRepository;
    }

    public async Task<OccurrenceResponse> Execute(int id, UpdateOccurrenceRequest request)
    {
        var occurrence = await _occurrenceRepository.GetByIdAsync(id);
        if (occurrence == null)
            throw new KeyNotFoundException("Ocorrência não encontrada");

        occurrence.Update(request.Title, request.Description, request.Status, request.Priority, request.AssignedToUserId);
        var updated = await _occurrenceRepository.UpdateAsync(occurrence);

        return new OccurrenceResponse
        {
            Id = updated.Id,
            Title = updated.Title,
            Description = updated.Description,
            Status = updated.Status,
            Priority = updated.Priority,
            ChatId = updated.ChatId,
            AssignedToUserId = updated.AssignedToUserId,
            CreatedByUserId = updated.CreatedByUserId,
            CreatedAt = updated.CreatedAt,
            LastUpdate = updated.LastUpdate
        };
    }
}
