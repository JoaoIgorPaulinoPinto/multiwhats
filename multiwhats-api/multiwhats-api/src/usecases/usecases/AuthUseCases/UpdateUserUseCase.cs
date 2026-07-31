using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.helpers;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.AuthInterfaces;

namespace multiwhats_api.src.usecases.usecases.AuthUseCases;

// Updates a user's name, password, role, and/or active status.
public class UpdateUserUseCase : IUpdateUserUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly UseCaseLogger _useCaseLogger;
    private readonly SystemConfigService _config;

    public UpdateUserUseCase(
        IUserRepository userRepository,
        UseCaseLogger useCaseLogger,
        SystemConfigService config)
    {
        _userRepository = userRepository;
        _useCaseLogger = useCaseLogger;
        _config = config;
    }

    public async Task<UserResponse> Execute(int targetUserId, UpdateUserRequest request)
    {
        var user = await _userRepository.GetByIdAsync(targetUserId)
            ?? throw new InvalidOperationException("Usuário não encontrado.");

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            if (request.Name.Length > 200)
                throw new InvalidOperationException("O nome deve ter no máximo 200 caracteres.");

            var existing = await _userRepository.GetByNameAsync(request.Name);
            if (existing != null && existing.Id != targetUserId)
                throw new InvalidOperationException("Já existe um usuário com este nome.");

            user.ChangeName(request.Name.Trim());
        }

        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            var minLength = await _config.GetIntAsync("Auth:PasswordMinLength", 6);
            if (request.NewPassword.Length < minLength)
                throw new InvalidOperationException($"A senha deve ter no mínimo {minLength} caracteres.");

            user.ChangePassword(PasswordHelper.Hash(request.NewPassword));
        }

        if (request.Role.HasValue)
            user.ChangeRole(request.Role.Value);

        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value)
                user.Activate();
            else
                user.Deactivate();
        }

        var updated = await _userRepository.UpdateAsync(user);

        await _useCaseLogger.LogAsync(
            action: "UpdateUser",
            entityType: "User",
            entityId: updated.Id,
            description: $"Updated user \"{updated.Name}\" (Role: {updated.Role}, Active: {updated.IsActive})"
        );

        return new UserResponse
        {
            Id = updated.Id,
            Name = updated.Name,
            Role = updated.Role.ToString(),
            IsActive = updated.IsActive,
            CreatedAt = updated.CreatedAt
        };
    }
}
