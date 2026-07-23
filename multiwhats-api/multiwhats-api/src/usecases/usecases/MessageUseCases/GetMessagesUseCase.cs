using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.helpers;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.MessageInterfaces;

namespace multiwhats_api.src.usecases.usecases.MessageUseCases;

/// <summary>
/// USE CASE DE CONSULTA DE MENSAGENS.
/// 
/// RESPONSABILIDADES:
/// - ExecuteAll: lista todas as mensagens
/// - ExecuteById: detalha uma mensagem específica
/// - ExecuteByChat: lista mensagens de um chat (paginado, ordem cronológica)
/// - ExecuteByPhoneNumber: lista mensagens de um telefone específico
/// 
/// PECULIARIDADE: MAPEAMENTO STATIC (compartilhado entre Use Cases)
/// - Os métodos MapToSummaryResponse e MapToDetailResponse são `internal static`
/// - Isso permite que OUTROS Use Cases (como SendMessageUseCase e SaveIncomingMessageUseCase)
///   usem esses mesmos métodos de mapeamento
/// - Evita duplicação de código
/// 
/// PAGINAÇÃO DO CHAT:
/// - As mensagens de um chat são paginadas (padrão: 50 por página)
/// - São retornadas em ordem cronológica (mais antiga primeiro)
/// - O Repository inverte a ordem: busca DESC do banco → inverte para ASC na memória
/// 
/// SANITIZAÇÃO DE TELEFONE:
/// - O ExecuteByPhoneNumber usa PhoneNumberHelper.Sanitize() para limpar o telefone
/// - Isso garante que "(11) 99999-9999" e "11999999999" encontrem as mesmas mensagens
/// </summary>
public class GetMessagesUseCase : IGetMessagesUseCase
{
    private readonly IMessageRepository _messageRepository;
    private readonly UseCaseLogger _useCaseLogger;

    public GetMessagesUseCase(IMessageRepository messageRepository, UseCaseLogger useCaseLogger)
    {
        _messageRepository = messageRepository;
        _useCaseLogger = useCaseLogger;
    }

    /// <summary>
    /// Lista todas as mensagens do sistema.
    /// Útil para debug e auditoria (geralmente não usado no Frontend).
    /// </summary>
    public async Task<List<MessageSummaryResponse>> ExecuteAll()
    {
        var messages = await _messageRepository.GetAllAsync();

        await _useCaseLogger.LogAsync(
            action: "GetMessages",
            entityType: "Message",
            entityId: null,
            description: $"Listed all messages (count: {messages.Count})"
        );

        return messages.Select(MapToSummaryResponse).ToList();
    }

    /// <summary>
    /// Detalha uma mensagem específica.
    /// Retorna todos os campos incluindo: JID, timestamp, mídia, status de entrega, etc.
    /// </summary>
    public async Task<MessageDetailResponse?> ExecuteById(int id)
    {
        var message = await _messageRepository.GetByIdAsync(id);

        await _useCaseLogger.LogAsync(
            action: "GetMessage",
            entityType: "Message",
            entityId: id,
            description: message != null
                ? $"Retrieved message #{id} (from: {message.FromJid})"
                : $"Message #{id} not found"
        );

        return message != null ? MapToDetailResponse(message) : null;
    }

    /// <summary>
    /// Lista mensagens de um chat com paginação.
    /// 
    /// PAGINAÇÃO:
    /// - page: qual página (começa em 1)
    /// - pageSize: itens por página (padrão: 50)
    /// - Response: PaginatedResponse com items, totalCount, page, pageSize, totalPages
    /// 
    /// ORDEM:
    /// - Mensagens são retornadas em ordem cronológica (mais antiga primeiro)
    /// - O Repository busca DESC do banco e inverte para ASC na memória
    /// </summary>
    public async Task<PaginatedResponse<MessageSummaryResponse>> ExecuteByChat(int chatId, int page, int pageSize)
    {
        var messages = await _messageRepository.GetByChatAsync(chatId, page, pageSize);
        var totalCount = await _messageRepository.GetByChatTotalCountAsync(chatId);

        await _useCaseLogger.LogAsync(
            action: "GetMessages",
            entityType: "Message",
            entityId: null,
            description: $"Listed messages for chat #{chatId} (page {page}, pageSize {pageSize}, total {totalCount})"
        );

        return new PaginatedResponse<MessageSummaryResponse>
        {
            Items = messages.Select(MapToSummaryResponse).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    /// <summary>
    /// Lista todas as mensagens de um número de telefone específico.
    /// 
    /// SANITIZAÇÃO:
    /// - Remove caracteres especiais do telefone antes de buscar
    /// - Exemplo: "(11) 99999-9999" → "11999999999"
    /// - Isso garante que a busca encontre todas as mensagens independente da formatação
    /// </summary>
    public async Task<List<MessageSummaryResponse>> ExecuteByPhoneNumber(string phoneNumber)
    {
        var sanitized = PhoneNumberHelper.Sanitize(phoneNumber);
        var messages = await _messageRepository.GetByPhoneNumberAsync(sanitized);

        await _useCaseLogger.LogAsync(
            action: "GetMessages",
            entityType: "Message",
            entityId: null,
            description: $"Listed messages for phone {sanitized} (count: {messages.Count})"
        );

        return messages.Select(MapToSummaryResponse).ToList();
    }

    // ═══════════════════════════════════════════════════════════════════
    // MÉTODOS DE MAPEAMENTO ESTÁTICOS (compartilhados entre Use Cases)
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Converte uma entidade Message para MessageSummaryResponse (resumo).
    /// Usado em listas e respostas paginadas.
    /// 
    /// Inclui: body, direction, type, sentAt, mídia, deliveryStatus, chatId
    /// NÃO inclui: JIDs, timestamp, userId, occurrenceId (são detalhes)
    /// </summary>
    internal static MessageSummaryResponse MapToSummaryResponse(Message message)
    {
        return new MessageSummaryResponse
        {
            Id = message.Id,
            Body = message.Body,
            Direction = message.Direction,
            Type = message.Type,
            SentAt = message.SentAt,
            PhoneNumber = message.PhoneNumber,
            NotifyName = message.NotifyName,
            HasMedia = message.HasMedia,
            MediaUrl = message.MediaUrl,
            MediaMimeType = message.MediaMimeType,
            MediaFilename = message.MediaFilename,
            MediaSize = message.MediaSize,
            MediaCaption = message.MediaCaption,
            DeliveryStatus = message.DeliveryStatus,
            ChatId = message.ChatId,
            CreatedAt = message.CreatedAt
        };
    }

    /// <summary>
    /// Converte uma entidade Message para MessageDetailResponse (detalhes completos).
    /// Usado quando retorna uma única mensagem.
    /// 
    /// Inclui TUDO: JIDs, timestamp unix, userId, occurrenceId, replyToId, isForwarded
    /// </summary>
    internal static MessageDetailResponse MapToDetailResponse(Message message)
    {
        return new MessageDetailResponse
        {
            Id = message.Id,
            MessageId = message.MessageId,
            FromJid = message.FromJid,
            ToJid = message.ToJid,
            PhoneNumber = message.PhoneNumber,
            Body = message.Body,
            Direction = message.Direction,
            Type = message.Type,
            Timestamp = message.Timestamp,
            SentAt = message.SentAt,
            NotifyName = message.NotifyName,
            HasMedia = message.HasMedia,
            MediaUrl = message.MediaUrl,
            MediaMimeType = message.MediaMimeType,
            MediaFilename = message.MediaFilename,
            MediaSize = message.MediaSize,
            MediaCaption = message.MediaCaption,
            DeliveryStatus = message.DeliveryStatus,
            IsForwarded = message.IsForwarded,
            ChatId = message.ChatId,
            UserId = message.UserId,
            OccurrenceId = message.OccurrenceId,
            ReplyToId = message.ReplyToId,
            CreatedAt = message.CreatedAt
        };
    }
}
