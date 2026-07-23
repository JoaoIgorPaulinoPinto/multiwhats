using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.usecases.interfaces.AuthInterfaces;

namespace multiwhats_api.src.controllers;

/// <summary>
/// CONTROLLER DE AUTENTICAÇÃO.
/// 
/// Endpoints: /api/auth/*
/// 
/// RESPONSABILIDADES:
/// - POST /register: Criar novo usuário
/// - POST /login: Fazer login e retornar JWT
/// - POST /logout: Revogar token (adicionar à blacklist)
/// 
/// PADRÃO DOS CONTROLLERS:
/// - Controllers são "receptores" de requisições HTTP
/// - São FINOS: recebem o pedido, delegam para o Use Case, e retornam a resposta
/// - NÃO contêm lógica de negócio (isso fica nos Use Cases)
/// 
/// ANATOMIA DE UM CONTROLLER:
/// - [ApiController]: Habilita validação automática de modelos
/// - [Route("api/auth")]: Define a rota base (ex: /api/auth/login)
/// - Cada método é um endpoint (GET, POST, PUT, DELETE, etc.)
/// - [Authorize]: Exige token JWT válido
/// - [AllowAnonymous]: Não exige autenticação (público)
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    // Dependências injetadas via DI (Dependency Injection)
    private readonly IRegisterUserUseCase _registerUserUseCase;
    private readonly ILoginUseCase _loginUseCase;
    private readonly ILogoutUseCase _logoutUseCase;

    public AuthController(
        IRegisterUserUseCase registerUserUseCase,
        ILoginUseCase loginUseCase,
        ILogoutUseCase logoutUseCase)
    {
        _registerUserUseCase = registerUserUseCase;
        _loginUseCase = loginUseCase;
        _logoutUseCase = logoutUseCase;
    }

    /// <summary>
    /// Registra um novo usuário no sistema.
    /// 
    /// POST /api/auth/register
    /// Body: { "name": "Joao", "password": "123123" }
    /// Response: 201 Created com o usuário criado
    /// Erro: 409 Conflict se o nome já existir
    /// 
    /// NOTA: Não tem [Authorize] → qualquer pessoa pode criar conta (público)
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        try
        {
            var user = await _registerUserUseCase.Execute(request);
            return Created("", new { message = "Usuário criado com sucesso.", user });
        }
        catch (InvalidOperationException ex)
        {
            // 409 = Conflict (o recurso já existe)
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Faz login e retorna um token JWT.
    /// 
    /// POST /api/auth/login
    /// Body: { "name": "Joao", "password": "123123" }
    /// Response: { "token": "eyJhbG...", "user": { "id": 1, "name": "Joao", "role": "Support" } }
    /// Erro: 401 Unauthorized se credenciais inválidas
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _loginUseCase.Execute(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            // 401 = Unauthorized (credenciais erradas)
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Faz logout revogando o token atual.
    /// 
    /// POST /api/auth/logout
    /// Headers: Authorization: Bearer {token}
    /// Response: { "message": "Logout realizado com sucesso." }
    /// 
    /// COMO FUNCIONA:
    /// 1. Extrai o token do header Authorization
    /// 2. Decodifica o JWT para obter o JTI (ID único do token)
    /// 3. Adiciona o JTI à blacklist (TokenBlacklistService)
    /// 4. A partir de agora, esse token é recusado mesmo que não tenha expirado
    /// </summary>
    [HttpPost("logout")]
    [Authorize]  // Precisa estar logado para fazer logout
    public async Task<IActionResult> Logout()
    {
        // Extrai o token do header Authorization: "Bearer eyJhbG..."
        var token = Request.Headers.Authorization.FirstOrDefault()?.Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token))
            return BadRequest(new { message = "Token não fornecido." });

        // Envia o token para o Use Case revogar
        await _logoutUseCase.Execute(User, token);
        return Ok(new { message = "Logout realizado com sucesso." });
    }
}
