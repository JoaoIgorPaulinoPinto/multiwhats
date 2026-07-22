using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.TaskInterfaces;

namespace multiwhats_api.src.usecases.usecases.TaskUseCases;

public class UpdateTaskStatusUseCase : IUpdateTaskStatusUseCase
{
    private readonly IClientTaskRepository _taskRepository;
    private readonly UseCaseLogger _useCaseLogger;

    public UpdateTaskStatusUseCase(IClientTaskRepository taskRepository, UseCaseLogger useCaseLogger)
    {
        _taskRepository = taskRepository;
        _useCaseLogger = useCaseLogger;
    }

    public async Task<TaskDetailResponse> Execute(int id, UpdateTaskStatusRequest request)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null)
            throw new KeyNotFoundException("Tarefa não encontrada");

        task.UpdateStatus(request.Status);
        var updated = await _taskRepository.UpdateAsync(task);

        await _useCaseLogger.LogAsync(
            action: "UpdateTaskStatus",
            entityType: "ClientTask",
            entityId: id,
            description: $"Updated task #{id} status to {updated.Status} (Title: {updated.Title})"
        );

        return GetTasksUseCase.MapToDetailResponse(updated);
    }
}
