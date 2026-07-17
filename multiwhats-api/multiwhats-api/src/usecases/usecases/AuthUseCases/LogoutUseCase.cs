using System.IdentityModel.Tokens.Jwt;
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
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
        if (jti != null)
        {
            _blacklist.Revoke(jti, jwtToken.ValidTo);
        }

        return Task.CompletedTask;
    }
}
