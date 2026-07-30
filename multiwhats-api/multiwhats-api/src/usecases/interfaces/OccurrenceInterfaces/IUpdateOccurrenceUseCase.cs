using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;

namespace multiwhats_api.src.usecases.interfaces.OccurrenceInterfaces;

public interface IUpdateOccurrenceUseCase
{
    Task<OccurrenceDetailResponse> Execute(int id, UpdateOccurrenceRequest request);
}
