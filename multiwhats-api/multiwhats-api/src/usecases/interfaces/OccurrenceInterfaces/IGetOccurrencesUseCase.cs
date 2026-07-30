using multiwhats_api.src.data.dtos.Responses;

namespace multiwhats_api.src.usecases.interfaces.OccurrenceInterfaces;

public interface IGetOccurrencesUseCase
{
    Task<List<OccurrenceListResponse>> ExecuteAll();
    Task<OccurrenceDetailResponse?> ExecuteById(int id);
    Task<List<ChatOccurrenceSummaryResponse>> ExecuteByChat(int chatId);
}
