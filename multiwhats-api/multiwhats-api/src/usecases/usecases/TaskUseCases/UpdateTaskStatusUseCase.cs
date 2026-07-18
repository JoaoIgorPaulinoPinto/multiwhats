using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.usecases.interfaces.TaskInterfaces;

namespace multiwhats_api.src.usecases.usecases.TaskUseCases;

public class UpdateTaskStatusUseCase : IUpdateTaskStatusUseCase
{
    private readonly IClientTaskRepository _taskRepository;

    public UpdateTaskStatusUseCase(IClientTaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<TaskResponse> Execute(int id, UpdateTaskStatusRequest request)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null)
            throw new KeyNotFoundException("Tarefa não encontrada");

        task.UpdateStatus(request.Status);
        var updated = await _taskRepository.UpdateAsync(task);

        return GetTasksUseCase.MapResponseStatic(updated);
    }
}
