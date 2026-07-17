using System.Security.Claims;

namespace multiwhats_api.src.usecases.interfaces.AuthInterfaces;

public interface ILogoutUseCase
{
    Task Execute(ClaimsPrincipal user, string token);
}
