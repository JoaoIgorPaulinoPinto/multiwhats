using Microsoft.AspNetCore.Identity;
using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.usecases.interfaces.AuthInterfaces;

namespace multiwhats_api.src.usecases.usecases.AuthUseCases;

public class RegistrarUsuarioUseCase : IRegistrarUsuarioUseCase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly PasswordHasher<Usuario> _passwordHasher;

    public RegistrarUsuarioUseCase(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = new PasswordHasher<Usuario>();
    }

    public async Task<UsuarioResponse> Execute(RegistrarUsuarioRequest request)
    {
        var existing = await _usuarioRepository.GetByNomeAsync(request.Nome);
        if (existing != null)
            throw new InvalidOperationException("Já existe um usuário com este e-mail.");

        var tempUsuario = new Usuario(request.Nome, request.Senha);
        var hashedSenha = _passwordHasher.HashPassword(tempUsuario, request.Senha);

        var usuario = new Usuario(request.Nome, hashedSenha);
        var created = await _usuarioRepository.AddAsync(usuario);

        return new UsuarioResponse
        {
            Id = created.Id,
            Nome = created.Nome,
            CreatedAt = created.CreatedAt
        };
    }
}
