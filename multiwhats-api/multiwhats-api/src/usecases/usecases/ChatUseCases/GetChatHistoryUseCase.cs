using Microsoft.EntityFrameworkCore;
using multiwhats_api.src.data.db;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.ChatInterfaces;

namespace multiwhats_api.src.usecases.usecases.ChatUseCases;

// Returns the full history of a chat: atendimento sessions (who started/ended and when),
// the occurrences linked to it, and a chronological timeline of everything that happened.
public class GetChatHistoryUseCase : IGetChatHistoryUseCase
{
    private readonly AppDbContext _context;
    private readonly IChatRepository _chatRepository;
    private readonly UseCaseLogger _useCaseLogger;

    public GetChatHistoryUseCase(
        AppDbContext context,
        IChatRepository chatRepository,
        UseCaseLogger useCaseLogger)
    {
        _context = context;
        _chatRepository = chatRepository;
        _useCaseLogger = useCaseLogger;
    }

    public async Task<ChatHistoryResponse?> Execute(int chatId)
    {
        var chat = await _chatRepository.GetByIdAsync(chatId);
        if (chat == null) return null;

        var chatLogs = await _context.AuditLogs
            .AsNoTracking()
            .Where(l => l.EntityType == "Chat"
                && l.EntityId == chatId
                && (l.Action == "AssignChat" || l.Action == "UnassignChat"))
            .OrderBy(l => l.Timestamp)
            .ThenBy(l => l.Id)
            .ToListAsync();

        var atendimentos = BuildAtendimentoSessions(chatLogs);

        var occurrences = await _context.Occurrences
            .AsNoTracking()
            .Include(o => o.CreatedBy)
            .Include(o => o.AssignedTo)
            .Where(o => o.ChatId == chatId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var occurrenceIds = occurrences.Select(o => o.Id).ToList();

        var occurrenceLogs = await _context.AuditLogs
            .AsNoTracking()
            .Where(l => l.EntityType == "Occurrence"
                && l.EntityId.HasValue
                && occurrenceIds.Contains(l.EntityId.Value))
            .OrderBy(l => l.Timestamp)
            .ThenBy(l => l.Id)
            .ToListAsync();

        var timeline = BuildTimeline(chatLogs, occurrenceLogs, occurrences);

        await _useCaseLogger.LogAsync(
            action: "GetChatHistory",
            entityType: "Chat",
            entityId: chatId,
            description: $"Retrieved history for chat #{chatId} ({atendimentos.Count} atendimentos, {occurrences.Count} ocorrências)"
        );

        return new ChatHistoryResponse
        {
            ChatId = chatId,
            Atendimentos = atendimentos,
            Occurrences = occurrences.Select(o => new ChatOccurrenceHistoryResponse
            {
                Id = o.Id,
                Title = o.Title,
                Description = o.Description,
                Status = o.Status,
                Priority = o.Priority,
                CreatedByUserId = o.CreatedByUserId,
                CreatedByName = o.CreatedBy?.Name,
                AssignedToUserId = o.AssignedToUserId,
                AssignedToName = o.AssignedTo?.Name,
                CreatedAt = o.CreatedAt,
                LastUpdate = o.LastUpdate
            }).ToList(),
            Timeline = timeline
        };
    }

    // Pairs AssignChat logs (start) with the next UnassignChat log (end) into sessions.
    private static List<ChatAtendimentoHistoryResponse> BuildAtendimentoSessions(List<AuditLog> logs)
    {
        var sessions = new List<ChatAtendimentoHistoryResponse>();
        AuditLog? openLog = null;
        var openId = 0;
        var counter = 0;

        foreach (var log in logs)
        {
            if (log.Action == "AssignChat")
            {
                // A re-assignment while a session is still open closes the previous one implicitly.
                if (openLog != null)
                {
                    sessions.Add(BuildSession(openLog, openId, log.Timestamp, null, null));
                    openLog = null;
                }

                counter++;
                openLog = log;
                openId = counter;
            }
            else if (log.Action == "UnassignChat" && openLog != null)
            {
                sessions.Add(BuildSession(openLog, openId, log.Timestamp, log.UserId, log.UserName));
                openLog = null;
            }
        }

        if (openLog != null)
            sessions.Add(BuildSession(openLog, openId, null, null, null));

        return sessions;
    }

    private static ChatAtendimentoHistoryResponse BuildSession(
        AuditLog assignLog,
        int id,
        DateTime? endedAt,
        int? endedByUserId,
        string? endedByName)
    {
        var isOpen = !endedAt.HasValue;
        return new ChatAtendimentoHistoryResponse
        {
            Id = id,
            StartedAt = assignLog.Timestamp,
            StartedByUserId = assignLog.UserId,
            StartedByName = assignLog.UserName,
            EndedAt = endedAt,
            EndedByUserId = endedByUserId,
            EndedByName = endedByName,
            IsOpen = isOpen,
            DurationSeconds = endedAt.HasValue && endedAt.Value > assignLog.Timestamp
                ? (long)(endedAt.Value - assignLog.Timestamp).TotalSeconds
                : null
        };
    }

    private static List<ChatHistoryTimelineItemResponse> BuildTimeline(
        List<AuditLog> chatLogs,
        List<AuditLog> occurrenceLogs,
        List<Occurrence> occurrences)
    {
        var items = new List<ChatHistoryTimelineItemResponse>();

        foreach (var log in chatLogs)
        {
            var started = log.Action == "AssignChat";
            items.Add(new ChatHistoryTimelineItemResponse
            {
                Type = started ? "AtendimentoIniciado" : "AtendimentoFinalizado",
                Title = started ? "Atendimento iniciado" : "Atendimento finalizado",
                Description = started
                    ? $"Atendimento iniciado por {FormatUser(log.UserName)}"
                    : $"Atendimento finalizado por {FormatUser(log.UserName)}",
                Timestamp = log.Timestamp,
                UserId = log.UserId,
                UserName = log.UserName
            });
        }

        var titleById = occurrences.ToDictionary(o => o.Id, o => o.Title);
        var loggedCreatedIds = new HashSet<int>();

        foreach (var log in occurrenceLogs)
        {
            var occurrenceId = log.EntityId ?? 0;
            var title = titleById.GetValueOrDefault(occurrenceId, $"Ocorrência #{occurrenceId}");
            var type = log.Action switch
            {
                "CreateOccurrence" => "OcorrenciaCriada",
                "DeleteOccurrence" => "OcorrenciaExcluida",
                _ => "OcorrenciaAtualizada"
            };

            if (log.Action == "CreateOccurrence")
                loggedCreatedIds.Add(occurrenceId);

            var label = log.Action switch
            {
                "CreateOccurrence" => "Ocorrência criada",
                "DeleteOccurrence" => "Ocorrência excluída",
                "AdvanceOccurrenceStatus" => "Status da ocorrência alterado",
                _ => "Ocorrência atualizada"
            };

            items.Add(new ChatHistoryTimelineItemResponse
            {
                Type = type,
                Title = label,
                Description = $"{label} \"{title}\" por {FormatUser(log.UserName)}",
                Timestamp = log.Timestamp,
                UserId = log.UserId,
                UserName = log.UserName,
                OccurrenceId = occurrenceId
            });
        }

        // Fallback so every occurrence appears at least once in the timeline
        // even if its creation was never written to the audit log.
        foreach (var occurrence in occurrences)
        {
            if (loggedCreatedIds.Contains(occurrence.Id))
                continue;

            items.Add(new ChatHistoryTimelineItemResponse
            {
                Type = "OcorrenciaCriada",
                Title = "Ocorrência criada",
                Description = $"Ocorrência criada \"{occurrence.Title}\" por {FormatUser(occurrence.CreatedBy?.Name)}",
                Timestamp = occurrence.CreatedAt,
                UserId = occurrence.CreatedByUserId,
                UserName = occurrence.CreatedBy?.Name,
                OccurrenceId = occurrence.Id
            });
        }

        return items
            .OrderBy(i => i.Timestamp)
            .ToList();
    }

    private static string FormatUser(string? name)
    {
        return string.IsNullOrWhiteSpace(name) ? "desconhecido" : name;
    }
}
