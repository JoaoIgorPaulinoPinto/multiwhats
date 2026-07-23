using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using multiwhats_api.src.data.db;
using multiwhats_api.src.data.entities;
using System.Security.Claims;

namespace multiwhats_api.src.services;

/// <summary>
/// LOGGER CENTRALIZADO PARA AUDITORIA DE USE CASES.
/// 
/// O QUE É:
/// - Registra cada ação importante realizada no sistema (login, criar cliente, enviar mensagem, etc.)
/// - Salva os logs no banco de dados (tabela AuditLogs)
/// - Envia os logs em TEMPO REAL para o Frontend via SignalR
/// 
/// DIFERENÇA DO AuditService:
/// - AuditService: gera logs AUTOMATICAMENTES baseado nas mudanças do Entity Framework
/// - UseCaseLogger: registra ações EXPLÍCITAS (o programador escolhe o que logar)
/// 
/// Por que dois sistemas?
/// - O UseCaseLogger é mais granular e controlado (sabe o contexto da ação)
/// - O AuditService é mais genérico (só sabe o que mudou, não por quê)
/// 
/// COMO FUNCIONA INTERNAMENTE:
/// 1. O Use Case chama: _useCaseLogger.LogAsync("CreateClient", "Client", 5, "Created Client #5")
/// 2. O logger cria um NOVO SCOPE (novo DbContext) para evitar problemas de threading
/// 3. Salva o AuditLog no banco de dados
/// 4. Envia o log para todos os clientes via SignalR (evento "LogReceived")
/// 5. O Frontend recebe e exibe no console/painel de auditoria
/// 
/// POR QUE CRIAR UM NOVO SCOPE:
/// - O UseCaseLogger é registrado como SINGLETON (uma instância para toda a aplicação)
/// - Mas o AppDbContext é SCOPED (uma instância por requisição)
/// - Se usássemos o mesmo DbContext, teríamos problemas de threading
/// - Então criamos um novo scope para cada log (isolamento total)
/// 
/// PADRÃO "FIRE AND FORGET":
/// - Os logs são salvos em background (não bloqueiam a resposta)
/// - Se falhar, o sistema ignora silenciosamente (catch vazio)
/// - Isso garante que um erro de log não quebre o sistema principal
/// </summary>
public class UseCaseLogger
{
    private readonly IServiceScopeFactory _scopeFactory;        // Para criar novos scopes
    private readonly IHttpContextAccessor _httpContextAccessor; // Para acessar o usuário atual
    private readonly IHubContext<WhatsappHub> _hubContext;      // Para enviar eventos via SignalR

    public UseCaseLogger(
        IServiceScopeFactory scopeFactory,
        IHttpContextAccessor httpContextAccessor,
        IHubContext<WhatsappHub> hubContext)
    {
        _scopeFactory = scopeFactory;
        _httpContextAccessor = httpContextAccessor;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Registra uma ação de auditoria no banco e envia para o Frontend.
    /// 
    /// PARÂMETROS:
    /// - action: tipo da ação (ex: "CreateClient", "SendMessage", "Login")
    /// - entityType: tipo da entidade (ex: "Client", "Message", "User")
    /// - entityId: ID da entidade (ex: 5 para o Client #5)
    /// - description: descrição legível (ex: "Created Client #5 (Timontec)")
    /// - explicitUserId: ID do usuário (opcional, senão extrai do JWT)
    /// - explicitUserName: nome do usuário (opcional)
    /// 
    /// EXEMPLO DE USO:
    /// await _useCaseLogger.LogAsync(
    ///     action: "CreateContact",
    ///     entityType: "Contact",
    ///     entityId: created.Id,
    ///     description: $"Created contact \"{created.Name}\" (Jid: {created.Jid})",
    ///     explicitUserId: userId
    /// );
    /// </summary>
    public async Task LogAsync(
        string action,
        string entityType,
        int? entityId,
        string description,
        int? explicitUserId = null,
        string? explicitUserName = null)
    {
        AuditLog? log = null;

        try
        {
            // Obtém o ID e nome do usuário
            // Se foram passados explicitamente, usa eles; senão, extrai do JWT
            var userId = explicitUserId ?? GetCurrentUserId();
            var userName = explicitUserName ?? GetCurrentUserName();
            var userRole = GetCurrentUserRole();

            // ── CRIA UM NOVO SCOPE PARA ACESSAR O BANCO ──
            // Isso evita problemas de threading (o UseCaseLogger é Singleton,
            // mas o DbContext é Scoped - um não pode usar o outro diretamente)
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // ── CRIA O LOG DE AUDITORIA ──
            log = new AuditLog
            {
                UserId = userId,
                UserName = userName,
                UserRole = userRole,
                EntityType = entityType,
                EntityId = entityId,
                Action = action,
                Description = description,
                Timestamp = DateTime.UtcNow
            };

            // Salva no banco de dados
            context.AuditLogs.Add(log);
            await context.SaveChangesAsync();
        }
        catch
        {
            // PADRÃO "FIRE AND FORGET": se falhar, ignora silenciosamente
            // Um erro de log NÃO deve quebrar a funcionalidade principal
        }

        // ── ENVIAR O LOG PARA O FRONTEND VIA SIGNALR ──
        if (log != null)
        {
            try
            {
                // Envia o evento "LogReceived" para TODOS os clientes conectados
                // O Frontend ouve esse evento e exibe o log em tempo real
                await _hubContext.Clients.All.SendAsync("LogReceived", log);
            }
            catch
            {
                // Se o SignalR falhar, também ignora (fire and forget)
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // MÉTODOS AUXILIARES PARA EXTRAIR INFORMAÇÕES DO USUÁRIO ATUAL
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Extrai o ID do usuário do JWT (token de autenticação).
    /// O JWT contém um claim chamado "NameIdentifier" que é o ID do usuário.
    /// </summary>
    private int? GetCurrentUserId()
    {
        var claim = _httpContextAccessor.HttpContext?.User?
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null ? int.Parse(claim) : null;
    }

    private string? GetCurrentUserName()
    {
        return _httpContextAccessor.HttpContext?.User?
            .FindFirst(ClaimTypes.Name)?.Value;
    }

    private string? GetCurrentUserRole()
    {
        return _httpContextAccessor.HttpContext?.User?
            .FindFirst(ClaimTypes.Role)?.Value;
    }
}
