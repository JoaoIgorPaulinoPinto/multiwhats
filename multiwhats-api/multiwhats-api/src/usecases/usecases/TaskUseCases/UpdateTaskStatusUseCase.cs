using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.TaskInterfaces;

namespace multiwhats_api.src.usecases.usecases.TaskUseCases;

/// <summary>
/// USE CASE DE ATUALIZAÇÃO DE STATUS DE TAREFA.
/// 
/// FLUXO:
/// 1. Busca a tarefa pelo ID no banco
/// 2. Atualiza apenas o STATUS da tarefa
/// 3. Salva no banco
/// 4. Registra na auditoria
/// 5. Retorna os dados atualizados
/// 
/// PECULIARIDADE: CONTROLE DE ACESSO POR ROLE
/// ⚠️ Apenas usuários com role "Admin" ou "Dev" podem usar este endpoint.
/// 
/// Isso é controlado no Controller (TasksController):
/// [Authorize(Roles = "Admin,Dev")]
/// 
/// POR QUE RESTRINGIR:
/// - Operadores "Support" não devem poder mudar o status de tarefas
/// - Apenas gestores (Admin/Dev) podem dar andamento, concluir ou cancelar
/// - Isso mantém o controle de workflow da empresa
/// 
/// DIFERENÇA DO UpdateTaskUseCase:
/// - UpdateTaskUseCase: atualiza TUDO (título, descrição, prioridade, etc.)
/// - UpdateTaskStatusUseCase: atualiza APENAS o status
/// 
/// STATUS DISPONÍVEIS:
/// - Open: tarefa criada, aguardando início
/// - InProgress: tarefa em andamento
/// - Completed: tarefa concluída
/// - Cancelled: tarefa cancelada
/// </summary>
public class UpdateTaskStatusUseCase : IUpdateTaskStatusUseCase
{
    private readonly IClientTaskRepository _taskRepository;
    private readonly UseCaseLogger _useCaseLogger;

    public UpdateTaskStatusUseCase(IClientTaskRepository taskRepository, UseCaseLogger useCaseLogger)
    {
        _taskRepository = taskRepository;
        _useCaseLogger = useCaseLogger;
    }

    /// <summary>
    /// Executa a atualização do status de uma tarefa.
    /// 
    /// PASSOS:
    /// 1. Busca a tarefa pelo ID
    /// 2. Se não existe → lança KeyNotFoundException (404)
    /// 3. Chama task.UpdateStatus() (método da entidade que valida o status)
    /// 4. Salva no banco
    /// 5. Registra auditoria: "Updated task #5 status to InProgress"
    /// 6. Retorna os dados atualizados
    /// </summary>
    public async Task<TaskDetailResponse> Execute(int id, UpdateTaskStatusRequest request)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null)
            throw new KeyNotFoundException("Tarefa não encontrada");

        // Atualiza o status (método da entidade que valida o valor)
        task.UpdateStatus(request.Status);

        // Salva no banco
        var updated = await _taskRepository.UpdateAsync(task);

        // Registra auditoria
        await _useCaseLogger.LogAsync(
            action: "UpdateTaskStatus",
            entityType: "ClientTask",
            entityId: id,
            description: $"Updated task #{id} status to {updated.Status} (Title: {updated.Title})"
        );

        // Retorna dados atualizados
        return GetTasksUseCase.MapToDetailResponse(updated);
    }
}
