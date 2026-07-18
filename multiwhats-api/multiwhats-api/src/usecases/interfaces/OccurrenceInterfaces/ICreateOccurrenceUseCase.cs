using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;

namespace multiwhats_api.src.usecases.interfaces.OccurrenceInterfaces;

public interface ICreateOccurrenceUseCase
{
    Task<OccurrenceResponse> Execute(CreateOccurrenceRequest request, int userId);
}
