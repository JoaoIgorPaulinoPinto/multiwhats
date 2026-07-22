using multiwhats_api.src.data.dtos.Responses;

namespace multiwhats_api.src.usecases.interfaces.TaskInterfaces;

public interface IGetTasksUseCase
{
    Task<List<TaskListResponse>> ExecuteAll();
    Task<TaskDetailResponse?> ExecuteById(int id);
    Task<List<TaskListResponse>> ExecuteByClient(int clientId);
}
