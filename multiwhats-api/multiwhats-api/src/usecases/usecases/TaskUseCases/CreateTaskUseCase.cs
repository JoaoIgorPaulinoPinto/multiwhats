using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.TaskInterfaces;

namespace multiwhats_api.src.usecases.usecases.TaskUseCases;

public class CreateTaskUseCase : ICreateTaskUseCase
{
    private readonly IClientTaskRepository _taskRepository;
    private readonly UseCaseLogger _useCaseLogger;

    public CreateTaskUseCase(IClientTaskRepository taskRepository, UseCaseLogger useCaseLogger)
    {
        _taskRepository = taskRepository;
        _useCaseLogger = useCaseLogger;
    }

    public async Task<TaskDetailResponse> Execute(CreateTaskRequest request, int userId)
    {
        var task = new ClientTask(
            request.Title,
            request.ClientId,
            userId,
            request.Description,
            request.Priority,
            request.DueDate
        );

        var created = await _taskRepository.AddAsync(task);

        await _useCaseLogger.LogAsync(
            action: "CreateTask",
            entityType: "ClientTask",
            entityId: created.Id,
            description: $"Created task \"{created.Title}\" (ClientId: {created.ClientId}, Priority: {created.Priority}, Status: {created.Status})",
            explicitUserId: userId
        );

        return new TaskDetailResponse
        {
            Id = created.Id,
            Title = created.Title,
            Description = created.Description,
            Status = created.Status,
            Priority = created.Priority,
            DueDate = created.DueDate,
            ClientId = created.ClientId,
            AssignedToUserId = created.AssignedToUserId,
            CreatedByUserId = created.CreatedByUserId,
            CreatedAt = created.CreatedAt,
            LastUpdate = created.LastUpdate
        };
    }
}
