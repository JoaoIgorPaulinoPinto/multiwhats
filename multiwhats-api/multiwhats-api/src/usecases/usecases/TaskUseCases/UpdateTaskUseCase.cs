using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.TaskInterfaces;

namespace multiwhats_api.src.usecases.usecases.TaskUseCases;

public class UpdateTaskUseCase : IUpdateTaskUseCase
{
    private readonly IClientTaskRepository _taskRepository;
    private readonly UseCaseLogger _useCaseLogger;

    public UpdateTaskUseCase(IClientTaskRepository taskRepository, UseCaseLogger useCaseLogger)
    {
        _taskRepository = taskRepository;
        _useCaseLogger = useCaseLogger;
    }

    public async Task<TaskDetailResponse> Execute(int id, UpdateTaskRequest request)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null)
            throw new KeyNotFoundException("Tarefa não encontrada");

        task.Update(request.Title, request.Description, request.Status, request.Priority, request.DueDate, request.AssignedToUserId);
        var updated = await _taskRepository.UpdateAsync(task);

        await _useCaseLogger.LogAsync(
            action: "UpdateTask",
            entityType: "ClientTask",
            entityId: id,
            description: $"Updated task #{id} (Title: \"{updated.Title}\", Status: {updated.Status}, Priority: {updated.Priority})"
        );

        return GetTasksUseCase.MapToDetailResponse(updated);
    }
}
