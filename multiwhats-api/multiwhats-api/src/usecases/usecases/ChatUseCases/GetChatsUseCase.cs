using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.ChatInterfaces;

namespace multiwhats_api.src.usecases.usecases.ChatUseCases;

/// <summary>
/// USE CASE DE CONSULTA DE CONVERSAS (CHATS).
/// 
/// RESPONSABILIDADES:
/// - ExecuteAll: lista conversas com paginação + contadores + resumos de ocorrências
/// - ExecuteById: detalha uma conversa com ocorrências completas
/// 
/// PECULIARIDADE: MONTAGEM COMPLEXA DA RESPOSTA
/// - Cada ChatListResponse inclui: nome, telefone, contato, cliente, última mensagem,
///   contagem de mensagens, e RESUMOS de ocorrências
/// - Isso requer MÚLTIPLOS acessos ao banco (N+1 query pattern)
/// - Para cada chat, busca: contagem de mensagens + lista de ocorrências
/// - Em produção, isso deveria ser otimizado com queries otimizadas
/// 
/// PAGINAÇÃO:
/// - page: qual página mostrar (começa em 1)
/// - pageSize: quantos itens por página (padrão: 20)
/// - Response inclui: items, totalCount, page, pageSize, totalPages
/// 
/// LÓGICA DE NOME DO CHAT:
/// - Usa uma cascata de fallback: Name → Contact.Name → PhoneNumber → "Desconhecido"
/// - Isso garante que sempre terá um nome para exibir
/// </summary>
public class GetChatsUseCase : IGetChatsUseCase
{
    private readonly IChatRepository _chatRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IOccurrenceRepository _occurrenceRepository;
    private readonly UseCaseLogger _useCaseLogger;

    public GetChatsUseCase(
        IChatRepository chatRepository,
        IMessageRepository messageRepository,
        IOccurrenceRepository occurrenceRepository,
        UseCaseLogger useCaseLogger)
    {
        _chatRepository = chatRepository;
        _messageRepository = messageRepository;
        _occurrenceRepository = occurrenceRepository;
        _useCaseLogger = useCaseLogger;
    }

    /// <summary>
    /// Lista todas as conversas com paginação.
    /// 
    /// Para CADA chat, busca:
    /// 1. Contagem total de mensagens
    /// 2. Lista de ocorrências (resumo)
    /// 
    /// Isso monta uma resposta rica para o Frontend exibir:
    /// - Nome do chat
    /// - Última mensagem
    /// - Quantidade de mensagens
    /// - Resumo das ocorrências (título, status, prioridade)
    /// </summary>
    public async Task<PaginatedResponse<ChatListResponse>> ExecuteAll(int page, int pageSize)
    {
        // Busca os chats da página atual
        var chats = await _chatRepository.GetAllAsync(page, pageSize);
        var totalCount = await _chatRepository.GetTotalCountAsync();

        var responses = new List<ChatListResponse>();

        // Para cada chat, busca informações adicionais (N+1 queries)
        foreach (var chat in chats)
        {
            // Contagem de mensagens deste chat
            var msgCount = await _messageRepository.GetByChatTotalCountAsync(chat.Id);

            // Lista de ocorrências deste chat
            var occurrences = await _occurrenceRepository.GetByChatAsync(chat.Id);

            // Monta resumo das ocorrências
            var occurrenceSummaries = occurrences.Select(o => new ChatOccurrenceSummaryResponse
            {
                Id = o.Id,
                Title = o.Title,
                Status = o.Status,
                Priority = o.Priority,
                AssignedToName = o.AssignedTo?.Name,
                MessageCount = o.Messages?.Count ?? 0,
                CreatedAt = o.CreatedAt
            }).ToList();

            // Monta a resposta do chat
            responses.Add(new ChatListResponse
            {
                Id = chat.Id,
                Jid = chat.Jid,
                Name = chat.Name ?? chat.Contact?.Name ?? chat.PhoneNumber ?? "Desconhecido",  // Cascata de fallback
                PhoneNumber = chat.PhoneNumber,
                ContactId = chat.ContactId,
                ContactName = chat.Contact?.Name,
                ClientId = chat.ClientId,
                ClientName = chat.Client?.Name,
                LastMessageAt = chat.LastMessageAt,
                LastMessageBody = chat.LastMessageBody,
                AssignedToUserName = chat.AssignedTo?.Name,
                MessageCount = msgCount,
                Occurrences = occurrenceSummaries,
                CreatedAt = chat.CreatedAt
            });
        }

        await _useCaseLogger.LogAsync(
            action: "GetChats",
            entityType: "Chat",
            entityId: null,
            description: $"Listed chats (page {page}, pageSize {pageSize}, total {totalCount})"
        );

        return new PaginatedResponse<ChatListResponse>
        {
            Items = responses,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    /// <summary>
    /// Detalha uma conversa específica com todas as suas ocorrências.
    /// 
    /// Diferente do ExecuteAll, aqui retorna:
    /// - Dados completos do chat
    /// - Lista de ocorrências COMPLETAS (não só resumo)
    /// - Contagem de mensagens e ocorrências
    /// </summary>
    public async Task<ChatDetailResponse?> ExecuteById(int id)
    {
        var chat = await _chatRepository.GetByIdAsync(id);
        if (chat == null) return null;

        var msgCount = await _messageRepository.GetByChatTotalCountAsync(chat.Id);
        var occurrences = await _occurrenceRepository.GetByChatAsync(chat.Id);

        // Monta lista de ocorrências COMPLETAS (com todos os detalhes)
        var occurrenceDetails = occurrences.Select(o => new OccurrenceDetailResponse
        {
            Id = o.Id,
            Title = o.Title,
            Description = o.Description,
            Status = o.Status,
            Priority = o.Priority,
            ChatId = o.ChatId,
            AssignedToUserId = o.AssignedToUserId,
            AssignedToName = o.AssignedTo?.Name,
            CreatedByUserId = o.CreatedByUserId,
            CreatedByName = o.CreatedBy?.Name,
            MessageCount = o.Messages?.Count ?? 0,
            CreatedAt = o.CreatedAt,
            LastUpdate = o.LastUpdate
        }).ToList();

        await _useCaseLogger.LogAsync(
            action: "GetChat",
            entityType: "Chat",
            entityId: id,
            description: $"Retrieved chat #{id} (Jid: {chat.Jid})"
        );

        return new ChatDetailResponse
        {
            Id = chat.Id,
            Jid = chat.Jid,
            PhoneNumber = chat.PhoneNumber,
            Name = chat.Name ?? chat.Contact?.Name ?? chat.PhoneNumber,
            ContactId = chat.ContactId,
            ContactName = chat.Contact?.Name,
            ClientId = chat.ClientId,
            ClientName = chat.Client?.Name,
            LastMessageAt = chat.LastMessageAt,
            LastMessageBody = chat.LastMessageBody,
            AssignedToUserId = chat.AssignedToUserId,
            AssignedToUserName = chat.AssignedTo?.Name,
            Occurrences = occurrenceDetails,
            CreatedByUserId = chat.CreatedByUserId,
            MessageCount = msgCount,
            OccurrenceCount = occurrences.Count,
            CreatedAt = chat.CreatedAt,
            LastUpdate = chat.LastUpdate
        };
    }
}
