using System.Security.Claims;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.AuthInterfaces;

namespace multiwhats_api.src.usecases.usecases.AuthUseCases;

public class LogoutUseCase : ILogoutUseCase
{
    private readonly TokenBlacklistService _blacklist;

    public LogoutUseCase(TokenBlacklistService blacklist)
    {
        _blacklist = blacklist;
    }

    public Task Execute(ClaimsPrincipal user, string token)
    {
        var jti = user.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)?.Value;
        if (jti != null)
        {
            _blacklist.Revoke(jti, DateTime.UtcNow.AddHours(8));
        }

        return Task.CompletedTask;
    }
}
