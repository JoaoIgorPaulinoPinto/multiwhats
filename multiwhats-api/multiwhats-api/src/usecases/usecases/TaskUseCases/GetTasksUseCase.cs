using multiwhats_api.src.data.entities;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.usecases.interfaces.TaskInterfaces;

namespace multiwhats_api.src.usecases.usecases.TaskUseCases;

public class GetTasksUseCase : IGetTasksUseCase
{
    private readonly IClientTaskRepository _taskRepository;

    public GetTasksUseCase(IClientTaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<List<TaskResponse>> ExecuteAll()
    {
        var tasks = await _taskRepository.GetAllAsync();
        return tasks.Select(MapResponseStatic).ToList();
    }

    public async Task<TaskResponse?> ExecuteById(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        return task != null ? MapResponseStatic(task) : null;
    }

    public async Task<List<TaskResponse>> ExecuteByClient(int clientId)
    {
        var tasks = await _taskRepository.GetByClientAsync(clientId);
        return tasks.Select(MapResponseStatic).ToList();
    }

    public static TaskResponse MapResponseStatic(ClientTask t) => new()
    {
        Id = t.Id,
        Title = t.Title,
        Description = t.Description,
        Status = t.Status,
        Priority = t.Priority,
        DueDate = t.DueDate,
        ClientId = t.ClientId,
        ClientName = t.Client?.Name,
        AssignedToUserId = t.AssignedToUserId,
        AssignedToName = t.AssignedTo?.Name,
        CreatedByUserId = t.CreatedByUserId,
        CreatedByName = t.CreatedBy?.Name,
        CreatedAt = t.CreatedAt,
        LastUpdate = t.LastUpdate
    };
}
