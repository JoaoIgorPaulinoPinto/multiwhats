using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.usecases.interfaces.TaskInterfaces;

namespace multiwhats_api.src.usecases.usecases.TaskUseCases;

public class DeleteTaskUseCase : IDeleteTaskUseCase
{
    private readonly IClientTaskRepository _taskRepository;

    public DeleteTaskUseCase(IClientTaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<bool> Execute(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null)
            throw new KeyNotFoundException("Tarefa não encontrada");

        return await _taskRepository.DeleteAsync(id);
    }
}
