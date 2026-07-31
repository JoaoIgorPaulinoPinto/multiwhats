using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.usecases.interfaces.AuthInterfaces;
using System.Security.Claims;

namespace multiwhats_api.src.controllers;

// Endpoints: /api/auth/* (POST register, login, logout, codes)
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IRegisterUserUseCase _registerUserUseCase;
    private readonly ILoginUseCase _loginUseCase;
    private readonly ILogoutUseCase _logoutUseCase;
    private readonly IGenerateRegistrationCodeUseCase _generateRegistrationCodeUseCase;

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    public AuthController(
        IRegisterUserUseCase registerUserUseCase,
        ILoginUseCase loginUseCase,
        ILogoutUseCase logoutUseCase,
        IGenerateRegistrationCodeUseCase generateRegistrationCodeUseCase)
    {
        _registerUserUseCase = registerUserUseCase;
        _loginUseCase = loginUseCase;
        _logoutUseCase = logoutUseCase;
        _generateRegistrationCodeUseCase = generateRegistrationCodeUseCase;
    }

    // POST /api/auth/register - Criar novo usuário
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
            return Conflict(new { message = ex.Message });
        }
    }

    // POST /api/auth/login - Login e retorno de JWT
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
            return Unauthorized(new { message = ex.Message });
        }
    }

    // POST /api/auth/codes - Gerar códigos de registro (Admin/Dev)
    [HttpPost("codes")]
    [Authorize(Roles = "Admin,Dev")]
    public async Task<IActionResult> GenerateCodes([FromBody] GenerateRegistrationCodeRequest request)
    {
        var codes = await _generateRegistrationCodeUseCase.Execute(request, UserId);
        return Ok(new { message = $"{request.Quantity} código(s) gerado(s).", codes });
    }

    // POST /api/auth/logout - Revogar token (logout)
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var token = Request.Headers.Authorization.FirstOrDefault()?.Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token))
            return BadRequest(new { message = "Token não fornecido." });

        await _logoutUseCase.Execute(User, token);
        return Ok(new { message = "Logout realizado com sucesso." });
    }
}
