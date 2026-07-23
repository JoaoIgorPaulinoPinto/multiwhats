using multiwhats_api.src.data.db;
using multiwhats_api.src.data.entities;

namespace multiwhats_api.src.services;

/// <summary>
/// SERVIÇO DE AUDITORIA BASEADO NO CHANGE TRACKER DO ENTITY FRAMEWORK.
/// 
/// DIFERENÇA DO UseCaseLogger:
/// - UseCaseLogger: registra ações EXPLÍCITAS (o programador escolhe o que logar)
/// - AuditService: gera logs AUTOMATICAMENTES baseado nas mudanças detectadas pelo EF Core
/// 
/// COMO FUNCIONA:
/// 1. O Use Case modifica entidades (adiciona, modifica ou deleta)
/// 2. O Use Case chama: await _auditService.SaveAuditLogsAsync()
/// 3. O AuditService examina o ChangeTracker do EF Core
/// 4. Gera AuditLogs para cada mudança detectada (com valores antigos/novos)
/// 5. Salva os logs no banco de dados
/// 
/// OBSERVAÇÃO ATUAL:
/// - Este serviço NÃO está sendo usado ativamente no código
/// - O sistema de auditoria principal é o UseCaseLogger
/// - Este serviço está disponível como alternativa/complemento
/// </summary>
public class AuditService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Salva logs de auditoria baseados nas mudanças pendentes no ChangeTracker.
    /// 
    /// Chame este método ANTES de SaveChanges() para capturar todas as mudanças.
    /// O método GenerateAuditLogs() do AppDbContext compara valores antigos e novos
    /// e gera logs detalhados em JSON.
    /// </summary>
    public async Task SaveAuditLogsAsync()
    {
        var userId = GetCurrentUserId();
        var userName = GetCurrentUserName();
        var userRole = GetCurrentUserRole();

        // Gera os logs baseado nas mudanças do ChangeTracker
        var logs = _context.GenerateAuditLogs(userId, userName, userRole);

        if (logs.Count > 0)
        {
            _context.AuditLogs.AddRange(logs);
            await _context.SaveChangesAsync();
        }
    }

    private int? GetCurrentUserId()
    {
        var claim = _httpContextAccessor.HttpContext?.User?
            .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return claim != null ? int.Parse(claim) : null;
    }

    private string? GetCurrentUserName()
    {
        return _httpContextAccessor.HttpContext?.User?
            .FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
    }

    private string? GetCurrentUserRole()
    {
        return _httpContextAccessor.HttpContext?.User?
            .FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
    }
}
