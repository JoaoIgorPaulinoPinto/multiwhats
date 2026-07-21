using System.Security.Claims;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.AuthInterfaces;

namespace multiwhats_api.src.usecases.usecases.AuthUseCases;

public class LogoutUseCase : ILogoutUseCase
{
    private readonly TokenBlacklistService _blacklist;
    private readonly UseCaseLogger _useCaseLogger;

    public LogoutUseCase(TokenBlacklistService blacklist, UseCaseLogger useCaseLogger)
    {
        _blacklist = blacklist;
        _useCaseLogger = useCaseLogger;
    }

    public async Task Execute(ClaimsPrincipal user, string token)
    {
        var jti = user.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)?.Value;
        if (jti != null)
        {
            _blacklist.Revoke(jti, DateTime.UtcNow.AddHours(8));
        }

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = user.FindFirst(ClaimTypes.Name)?.Value;

        await _useCaseLogger.LogAsync(
            action: "Logout",
            entityType: "User",
            entityId: userId != null ? int.Parse(userId) : null,
            description: $"User \"{userName ?? "unknown"}\" logged out (token revoked)",
            explicitUserId: userId != null ? int.Parse(userId) : null,
            explicitUserName: userName
        );
    }
}
