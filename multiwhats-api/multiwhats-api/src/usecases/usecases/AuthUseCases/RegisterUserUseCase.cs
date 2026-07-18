using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.usecases.interfaces.AuthInterfaces;

namespace multiwhats_api.src.usecases.usecases.AuthUseCases;

public class RegisterUserUseCase : IRegisterUserUseCase
{
    private readonly IUserRepository _userRepository;

    public RegisterUserUseCase(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserResponse> Execute(RegisterUserRequest request)
    {
        var existing = await _userRepository.GetByNameAsync(request.Name);
        if (existing != null)
            throw new InvalidOperationException("Já existe um usuário com este nome.");

        var user = new User(request.Name, request.Password);
        var created = await _userRepository.AddAsync(user);

        return new UserResponse
        {
            Id = created.Id,
            Name = created.Name,
            Role = created.Role,
            IsActive = created.IsActive,
            CreatedAt = created.CreatedAt
        };
    }
}
