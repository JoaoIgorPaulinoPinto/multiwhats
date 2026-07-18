using multiwhats_api.src.data.dtos.Responses;

namespace multiwhats_api.src.usecases.interfaces.TaskInterfaces;

public interface IGetTasksUseCase
{
    Task<List<TaskResponse>> ExecuteAll();
    Task<TaskResponse?> ExecuteById(int id);
    Task<List<TaskResponse>> ExecuteByClient(int clientId);
}
