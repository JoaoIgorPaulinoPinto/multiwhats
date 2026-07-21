using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.AuthInterfaces;

namespace multiwhats_api.src.usecases.usecases.AuthUseCases;

public class LoginUseCase : ILoginUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly TokenService _tokenService;
    private readonly UseCaseLogger _useCaseLogger;

    public LoginUseCase(IUserRepository userRepository, TokenService tokenService, UseCaseLogger useCaseLogger)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _useCaseLogger = useCaseLogger;
    }

    public async Task<LoginResponse> Execute(LoginRequest request)
    {
        var user = await _userRepository.GetByNameAsync(request.Name);
        if (user == null || user.Password != request.Password || !user.IsActive)
            throw new UnauthorizedAccessException("Credenciais inválidas ou usuário inativo.");

        var token = _tokenService.GenerateToken(user);

        await _useCaseLogger.LogAsync(
            action: "Login",
            entityType: "User",
            entityId: user.Id,
            description: $"User \"{user.Name}\" logged in (Role: {user.Role})"
        );

        return new LoginResponse
        {
            Token = token,
            User = new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            }
        };
    }
}
