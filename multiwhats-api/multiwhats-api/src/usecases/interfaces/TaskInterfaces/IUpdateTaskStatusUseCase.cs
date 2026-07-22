using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;

namespace multiwhats_api.src.usecases.interfaces.TaskInterfaces;

public interface IUpdateTaskStatusUseCase
{
    Task<TaskDetailResponse> Execute(int id, UpdateTaskStatusRequest request);
}
