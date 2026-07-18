using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.usecases.interfaces.TaskInterfaces;

namespace multiwhats_api.src.usecases.usecases.TaskUseCases;

public class UpdateTaskUseCase : IUpdateTaskUseCase
{
    private readonly IClientTaskRepository _taskRepository;

    public UpdateTaskUseCase(IClientTaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<TaskResponse> Execute(int id, UpdateTaskRequest request)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null)
            throw new KeyNotFoundException("Tarefa não encontrada");

        task.Update(request.Title, request.Description, request.Status, request.Priority, request.DueDate, request.AssignedToUserId);
        var updated = await _taskRepository.UpdateAsync(task);

        return GetTasksUseCase.MapResponseStatic(updated);
    }
}
