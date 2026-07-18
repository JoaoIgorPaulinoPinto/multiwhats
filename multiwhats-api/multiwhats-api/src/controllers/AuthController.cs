using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.usecases.interfaces.AuthInterfaces;

namespace multiwhats_api.src.controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IRegistrarUsuarioUseCase _registrarUsuarioUseCase;
    private readonly ILogarUsuarioUseCase _logarUsuarioUseCase;
    private readonly ILogoutUseCase _logoutUseCase;

    public AuthController(
        IRegistrarUsuarioUseCase registrarUsuarioUseCase,
        ILogarUsuarioUseCase logarUsuarioUseCase,
        ILogoutUseCase logoutUseCase)
    {
        _registrarUsuarioUseCase = registrarUsuarioUseCase;
        _logarUsuarioUseCase = logarUsuarioUseCase;
        _logoutUseCase = logoutUseCase;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegistrarUsuarioRequest request)
    {
        try
        {
            var usuario = await _registrarUsuarioUseCase.Execute(request);
            return Created("", new { message = "Usuário criado com sucesso.", usuario });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _logarUsuarioUseCase.Execute(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

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
