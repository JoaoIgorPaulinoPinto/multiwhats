using multiwhats_api.src.data.entities;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.TaskInterfaces;

namespace multiwhats_api.src.usecases.usecases.TaskUseCases;

public class GetTasksUseCase : IGetTasksUseCase
{
    private readonly IClientTaskRepository _taskRepository;
    private readonly UseCaseLogger _useCaseLogger;

    public GetTasksUseCase(IClientTaskRepository taskRepository, UseCaseLogger useCaseLogger)
    {
        _taskRepository = taskRepository;
        _useCaseLogger = useCaseLogger;
    }

    public async Task<List<TaskListResponse>> ExecuteAll()
    {
        var tasks = await _taskRepository.GetAllAsync();

        await _useCaseLogger.LogAsync(
            action: "GetTasks",
            entityType: "ClientTask",
            entityId: null,
            description: $"Listed all tasks (count: {tasks.Count})"
        );

        return tasks.Select(MapToListResponse).ToList();
    }

    public async Task<TaskDetailResponse?> ExecuteById(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id);

        await _useCaseLogger.LogAsync(
            action: "GetTask",
            entityType: "ClientTask",
            entityId: id,
            description: task != null
                ? $"Retrieved task #{id} (Title: {task.Title}, Status: {task.Status})"
                : $"Task #{id} not found"
        );

        return task != null ? MapToDetailResponse(task) : null;
    }

    public async Task<List<TaskListResponse>> ExecuteByClient(int clientId)
    {
        var tasks = await _taskRepository.GetByClientAsync(clientId);

        await _useCaseLogger.LogAsync(
            action: "GetTasks",
            entityType: "ClientTask",
            entityId: null,
            description: $"Listed tasks for client #{clientId} (count: {tasks.Count})"
        );

        return tasks.Select(MapToListResponse).ToList();
    }

    internal static TaskListResponse MapToListResponse(ClientTask t) => new()
    {
        Id = t.Id,
        Title = t.Title,
        Status = t.Status,
        Priority = t.Priority,
        DueDate = t.DueDate,
        ClientName = t.Client?.Name,
        AssignedToName = t.AssignedTo?.Name,
        CreatedAt = t.CreatedAt
    };

    internal static TaskDetailResponse MapToDetailResponse(ClientTask t) => new()
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
