using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.TaskInterfaces;

namespace multiwhats_api.src.usecases.usecases.TaskUseCases;

public class DeleteTaskUseCase : IDeleteTaskUseCase
{
    private readonly IClientTaskRepository _taskRepository;
    private readonly UseCaseLogger _useCaseLogger;

    public DeleteTaskUseCase(IClientTaskRepository taskRepository, UseCaseLogger useCaseLogger)
    {
        _taskRepository = taskRepository;
        _useCaseLogger = useCaseLogger;
    }

    public async Task<bool> Execute(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null)
            throw new KeyNotFoundException("Tarefa não encontrada");

        var result = await _taskRepository.DeleteAsync(id);

        await _useCaseLogger.LogAsync(
            action: "DeleteTask",
            entityType: "ClientTask",
            entityId: id,
            description: $"Deleted task #{id} (Title: {task.Title})"
        );

        return result;
    }
}
