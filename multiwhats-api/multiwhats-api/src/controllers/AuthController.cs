using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.usecases.interfaces.AuthInterfaces;

namespace multiwhats_api.src.controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IRegistrarUsuarioUseCase _registrarUsuarioUseCase;
    private readonly ILogarUsuarioUseCase _logarUsuarioUseCase;

    public AuthController(
        IRegistrarUsuarioUseCase registrarUsuarioUseCase,
        ILogarUsuarioUseCase logarUsuarioUseCase)
    {
        _registrarUsuarioUseCase = registrarUsuarioUseCase;
        _logarUsuarioUseCase = logarUsuarioUseCase;
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
}
