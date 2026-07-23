using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.OccurrenceInterfaces;

namespace multiwhats_api.src.usecases.usecases.OccurrenceUseCases;

/// <summary>
/// USE CASE DE CRIAÇÃO DE OCORRÊNCIA (CHAMADO/INCIDENTE).
/// 
/// FLUXO:
/// 1. Cria a entidade Occurrence com os dados informados
/// 2. Salva no banco MySQL (vinculada a um Chat)
/// 3. Registra na auditoria
/// 4. Retorna os dados da ocorrência criada
/// 
/// O QUE É UMA OCORRÊNCIA:
/// - É um "chamado" ou "incidente" vinculado a uma conversa (Chat)
/// - Exemplo: "Cliente reclamou que boleto não foi gerado" → Occurrence vinculada ao Chat do cliente
/// - Cada ocorrência tem: título, descrição, prioridade (Low/Medium/High/Urgent)
/// - Status: Open → InProgress → Resolved → Closed
/// 
/// RELACIONAMENTO:
/// - Uma Occurrence SEMPRE está vinculada a um Chat (ChatId é obrigatório)
/// - Um Chat pode ter MUITAS Occurrences
/// - Uma Occurrence pode ter MUITAS Messages vinculadas
/// 
/// STATUS INICIAL:
/// - Ao criar, o status padrão é "Open" (definido na entidade Occurrence)
/// - O operador pode mudar depois via UpdateOccurrenceUseCase
/// 
/// PRIORIDADE:
/// - Low: sem urgência
/// - Medium: importância normal
/// - High: urgente, precisa de atenção
/// - Urgent: emergência, resolver imediatamente
/// </summary>
public class CreateOccurrenceUseCase : ICreateOccurrenceUseCase
{
    private readonly IOccurrenceRepository _occurrenceRepository;
    private readonly UseCaseLogger _useCaseLogger;

    public CreateOccurrenceUseCase(IOccurrenceRepository occurrenceRepository, UseCaseLogger useCaseLogger)
    {
        _occurrenceRepository = occurrenceRepository;
        _useCaseLogger = useCaseLogger;
    }

    /// <summary>
    /// Executa a criação de uma ocorrência.
    /// 
    /// PASSOS:
    /// 1. Cria entidade Occurrence com título, descrição, prioridade e ChatId
    /// 2. Salva no banco (o status inicial é "Open")
    /// 3. Registra auditoria: "Created occurrence 'Problema boleto' (ChatId: 1, Priority: High)"
    /// 4. Retorna os dados completos da ocorrência
    /// </summary>
    public async Task<OccurrenceDetailResponse> Execute(CreateOccurrenceRequest request, int userId)
    {
        // Cria a entidade Occurrence
        var occurrence = new Occurrence(
            request.Title,
            request.ChatId,        // Obrigatório: toda ocorrência pertence a um Chat
            userId,
            request.Description,
            request.Priority       // Padrão: Medium (definido no DTO)
        );

        // Salva no banco MySQL
        var created = await _occurrenceRepository.AddAsync(occurrence);

        // Registra auditoria
        await _useCaseLogger.LogAsync(
            action: "CreateOccurrence",
            entityType: "Occurrence",
            entityId: created.Id,
            description: $"Created occurrence \"{created.Title}\" (ChatId: {created.ChatId}, Priority: {created.Priority}, Status: {created.Status})",
            explicitUserId: userId
        );

        // Retorna os dados completos
        return new OccurrenceDetailResponse
        {
            Id = created.Id,
            Title = created.Title,
            Description = created.Description,
            Status = created.Status,
            Priority = created.Priority,
            ChatId = created.ChatId,
            AssignedToUserId = created.AssignedToUserId,
            CreatedByUserId = created.CreatedByUserId,
            CreatedAt = created.CreatedAt,
            LastUpdate = created.LastUpdate
        };
    }
}
