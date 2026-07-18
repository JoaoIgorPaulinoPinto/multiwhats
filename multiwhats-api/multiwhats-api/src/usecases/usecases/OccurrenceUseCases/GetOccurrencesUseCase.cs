using multiwhats_api.src.data.entities;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.usecases.interfaces.OccurrenceInterfaces;

namespace multiwhats_api.src.usecases.usecases.OccurrenceUseCases;

public class GetOccurrencesUseCase : IGetOccurrencesUseCase
{
    private readonly IOccurrenceRepository _occurrenceRepository;

    public GetOccurrencesUseCase(IOccurrenceRepository occurrenceRepository)
    {
        _occurrenceRepository = occurrenceRepository;
    }

    public async Task<List<OccurrenceResponse>> ExecuteAll()
    {
        var occurrences = await _occurrenceRepository.GetAllAsync();
        return MapResponses(occurrences);
    }

    public async Task<OccurrenceResponse?> ExecuteById(int id)
    {
        var occurrence = await _occurrenceRepository.GetByIdAsync(id);
        return occurrence != null ? MapResponse(occurrence) : null;
    }

    public async Task<List<OccurrenceResponse>> ExecuteByContact(int contactId)
    {
        var occurrences = await _occurrenceRepository.GetByContactAsync(contactId);
        return MapResponses(occurrences);
    }

    private static OccurrenceResponse MapResponse(Occurrence o) => new()
    {
        Id = o.Id,
        Title = o.Title,
        Description = o.Description,
        Status = o.Status,
        Priority = o.Priority,
        ContactId = o.ContactId,
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
