using multiwhats_api.src.data.db;
using multiwhats_api.src.data.entities;

namespace multiwhats_api.src.services;

public class AuditService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task SaveAuditLogsAsync()
    {
        var userId = GetCurrentUserId();
        var userName = GetCurrentUserName();
        var userRole = GetCurrentUserRole();

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
