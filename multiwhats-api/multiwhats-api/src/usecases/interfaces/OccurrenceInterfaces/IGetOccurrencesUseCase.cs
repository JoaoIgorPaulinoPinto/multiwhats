using multiwhats_api.src.data.dtos.Responses;

namespace multiwhats_api.src.usecases.interfaces.OccurrenceInterfaces;

public interface IGetOccurrencesUseCase
{
    Task<List<OccurrenceResponse>> ExecuteAll();
    Task<OccurrenceResponse?> ExecuteById(int id);
    Task<List<OccurrenceResponse>> ExecuteByChat(int chatId);
}
