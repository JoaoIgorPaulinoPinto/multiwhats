using Microsoft.AspNetCore.Identity;
using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.AuthInterfaces;

namespace multiwhats_api.src.usecases.usecases.AuthUseCases;

public class LogarUsuarioUseCase : ILogarUsuarioUseCase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly TokenService _tokenService;
    private readonly PasswordHasher<Usuario> _passwordHasher;

    public LogarUsuarioUseCase(IUsuarioRepository usuarioRepository, TokenService tokenService)
    {
        _usuarioRepository = usuarioRepository;
        _tokenService = tokenService;
        _passwordHasher = new PasswordHasher<Usuario>();
    }

    public async Task<LoginResponse> Execute(LoginRequest request)
    {
        var usuario = await _usuarioRepository.GetByNomeAsync(request.Nome);
        if (usuario == null)
            throw new UnauthorizedAccessException("Nome de usuário ou senha inválidos.");

        var result = _passwordHasher.VerifyHashedPassword(usuario, usuario.Senha, request.Senha);
        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Usuario ou senha inválidos.");

        var token = _tokenService.GenerateToken(usuario.Id, usuario.Nome);

        return new LoginResponse
        {
            Token = token,
            Usuario = new UsuarioResponse
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                CreatedAt = usuario.CreatedAt
            }
        };
    }
}
