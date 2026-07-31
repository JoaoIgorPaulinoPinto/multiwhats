using multiwhats_api.src.data.dtos.Responses;

namespace multiwhats_api.src.usecases.interfaces.OccurrenceInterfaces;

public interface IGetOccurrenceMetricsUseCase
{
    Task<OccurrenceMetricsResponse> Execute();
}
