using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.helpers;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.AuthInterfaces;

namespace multiwhats_api.src.usecases.usecases.AuthUseCases;

public class RegisterUserUseCase : IRegisterUserUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IRegistrationCodeRepository _registrationCodeRepository;
    private readonly UseCaseLogger _useCaseLogger;
    private readonly SystemConfigService _config;

    public RegisterUserUseCase(
        IUserRepository userRepository,
        IRegistrationCodeRepository registrationCodeRepository,
        UseCaseLogger useCaseLogger,
        SystemConfigService config)
    {
        _userRepository = userRepository;
        _registrationCodeRepository = registrationCodeRepository;
        _useCaseLogger = useCaseLogger;
        _config = config;
    }

    public async Task<UserResponse> Execute(RegisterUserRequest request)
    {
        var minLength = await _config.GetIntAsync("Auth:PasswordMinLength", 6);
        if (request.Password.Length < minLength)
            throw new InvalidOperationException($"A senha deve ter no mínimo {minLength} caracteres.");

        var requireCode = await _config.GetBoolAsync("Auth:RequireRegistrationCode", false);
        if (requireCode)
        {
            if (string.IsNullOrWhiteSpace(request.RegistrationCode))
                throw new InvalidOperationException("Código de registro é obrigatório.");

            var code = await _registrationCodeRepository.GetTrackedByCodeAsync(request.RegistrationCode);
            if (code == null || !code.IsValid())
                throw new InvalidOperationException("Código de registro inválido ou expirado.");
        }

        var existing = await _userRepository.GetByNameAsync(request.Name);
        if (existing != null)
            throw new InvalidOperationException("Já existe um usuário com este nome.");

        var user = new User(request.Name, PasswordHelper.Hash(request.Password));
        var created = await _userRepository.AddAsync(user);

        if (requireCode && !string.IsNullOrWhiteSpace(request.RegistrationCode))
        {
            var code = await _registrationCodeRepository.GetTrackedByCodeAsync(request.RegistrationCode);
            if (code != null)
            {
                code.MarkAsUsed(created.Id);
                await _registrationCodeRepository.UpdateAsync(code);
            }
        }

        await _useCaseLogger.LogAsync(
            action: "RegisterUser",
            entityType: "User",
            entityId: created.Id,
            description: $"Registered user \"{created.Name}\" (Role: {created.Role})"
        );

        return new UserResponse
        {
            Id = created.Id,
            Name = created.Name,
            Role = created.Role.ToString(),
            IsActive = created.IsActive,
            CreatedAt = created.CreatedAt
        };
    }
}
