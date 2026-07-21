using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using multiwhats_api.src.data.db;
using multiwhats_api.src.data.entities;
using System.Security.Claims;

namespace multiwhats_api.src.services;

public class UseCaseLogger
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IHubContext<WhatsappHub> _hubContext;

    public UseCaseLogger(
        IServiceScopeFactory scopeFactory,
        IHttpContextAccessor httpContextAccessor,
        IHubContext<WhatsappHub> hubContext)
    {
        _scopeFactory = scopeFactory;
        _httpContextAccessor = httpContextAccessor;
        _hubContext = hubContext;
    }

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
            var userId = explicitUserId ?? GetCurrentUserId();
            var userName = explicitUserName ?? GetCurrentUserName();
            var userRole = GetCurrentUserRole();

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

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

            context.AuditLogs.Add(log);
            await context.SaveChangesAsync();
        }
        catch
        {
        }

        if (log != null)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("LogReceived", log);
            }
            catch
            {
            }
        }
    }

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
