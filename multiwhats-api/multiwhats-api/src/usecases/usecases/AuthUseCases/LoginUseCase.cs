using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.AuthInterfaces;

namespace multiwhats_api.src.usecases.usecases.AuthUseCases;

/// <summary>
/// USE CASE DE LOGIN.
/// 
/// FLUXO:
/// 1. Busca o usuário pelo nome no banco
/// 2. Verifica se a senha confere (comparação direta - ver PECULIARIDADE abaixo)
/// 3. Verifica se o usuário está ativo
/// 4. Gera um token JWT com os dados do usuário
/// 5. Registra o login na auditoria
/// 6. Retorna o token + dados do usuário
/// 
/// PECULIARIDADE: SENHA EM TEXTO PLANO
/// ⚠️ ATENÇÃO: As senhas são armazenadas e comparadas em TEXTO PLANO (sem hash)!
/// 
/// Isso é uma falha GRAVE de segurança em produção:
/// - Se o banco for comprometido, todas as senhas ficam expostas
/// - A prática correta é usar Bcrypt, Argon2 ou PBKDF2 para hashear senhas
/// - O hash é irreversível: mesmo se o banco for roubo, as senhas originais não são expostas
/// 
/// Para um sistema real, deveria usar:
/// - Bcrypt.net (pacote NuGet) ou
/// - Microsoft.AspNetCore.Identity (framework de identidade do ASP.NET)
/// 
/// Por enquanto, funciona para desenvolvimento/testes, mas NÃO deve ir para produção.
/// </summary>
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

    /// <summary>
    /// Executa o login do usuário.
    /// 
    /// PASSOS:
    /// 1. Busca usuário por nome → se não existe ou senha errada → 401
    /// 2. Verifica se está ativo → se inativo → 401
    /// 3. Gera token JWT com: ID, nome, role, expiração 8h
    /// 4. Registra na auditoria: "User Joao logged in (Role: Admin)"
    /// 5. Retorna: { token: "eyJhbG...", user: { id, name, role, ... } }
    /// </summary>
    public async Task<LoginResponse> Execute(LoginRequest request)
    {
        // Busca o usuário pelo nome
        var user = await _userRepository.GetByNameAsync(request.Name);

        // Verifica credenciais (comparação direta - sem hash!)
        // Se usuário não existe OU senha não confere OU está inativo → erro
        if (user == null || user.Password != request.Password || !user.IsActive)
            throw new UnauthorizedAccessException("Credenciais inválidas ou usuário inativo.");

        // Gera o token JWT com os dados do usuário
        var token = _tokenService.GenerateToken(user);

        // Registra o login na auditoria
        await _useCaseLogger.LogAsync(
            action: "Login",
            entityType: "User",
            entityId: user.Id,
            description: $"User \"{user.Name}\" logged in (Role: {user.Role})"
        );

        // Retorna o token + dados do usuário
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
