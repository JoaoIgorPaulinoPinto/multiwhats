using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;

namespace multiwhats_api.src.usecases.interfaces.TaskInterfaces;

public interface IUpdateTaskUseCase
{
    Task<TaskDetailResponse> Execute(int id, UpdateTaskRequest request);
}
