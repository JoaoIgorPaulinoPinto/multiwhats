using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;

namespace multiwhats_api.src.usecases.interfaces.OccurrenceInterfaces;

public interface ICreateOccurrenceUseCase
{
    Task<OccurrenceDetailResponse> Execute(CreateOccurrenceRequest request, int userId);
}
