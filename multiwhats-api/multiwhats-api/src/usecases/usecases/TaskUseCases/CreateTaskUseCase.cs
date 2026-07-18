using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.usecases.interfaces.TaskInterfaces;

namespace multiwhats_api.src.usecases.usecases.TaskUseCases;

public class CreateTaskUseCase : ICreateTaskUseCase
{
    private readonly IClientTaskRepository _taskRepository;

    public CreateTaskUseCase(IClientTaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<TaskResponse> Execute(CreateTaskRequest request, int userId)
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

        return new TaskResponse
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
