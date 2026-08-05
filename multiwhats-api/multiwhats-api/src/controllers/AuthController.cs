using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.AuthInterfaces;
using System.Security.Claims;

namespace multiwhats_api.src.controllers;

// Endpoints: /api/auth/* (POST register, login, logout, refresh, codes)
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IRegisterUserUseCase _registerUserUseCase;
    private readonly ILoginUseCase _loginUseCase;
    private readonly ILogoutUseCase _logoutUseCase;
    private readonly IGenerateRegistrationCodeUseCase _generateRegistrationCodeUseCase;
    private readonly IUserRepository _userRepository;
    private readonly TokenService _tokenService;
    private readonly TokenBlacklistService _blacklist;

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    public AuthController(
        IRegisterUserUseCase registerUserUseCase,
        ILoginUseCase loginUseCase,
        ILogoutUseCase logoutUseCase,
        IGenerateRegistrationCodeUseCase generateRegistrationCodeUseCase,
        IUserRepository userRepository,
        TokenService tokenService,
        TokenBlacklistService blacklist)
    {
        _registerUserUseCase = registerUserUseCase;
        _loginUseCase = loginUseCase;
        _logoutUseCase = logoutUseCase;
        _generateRegistrationCodeUseCase = generateRegistrationCodeUseCase;
        _userRepository = userRepository;
        _tokenService = tokenService;
        _blacklist = blacklist;
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

    // POST /api/auth/login - Login e retorno de JWT + refresh token
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

    // POST /api/auth/refresh - Renova o JWT usando o refresh token (sem exigir novo login)
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return BadRequest(new { message = "Refresh token não fornecido." });

        var result = _tokenService.ValidateRefreshToken(request.RefreshToken);
        if (result == null)
            return Unauthorized(new { message = "Refresh token inválido ou expirado." });

        if (_blacklist.IsRevoked(result.Jti))
            return Unauthorized(new { message = "Refresh token revogado." });

        var user = await _userRepository.GetByIdAsync(result.UserId);
        if (user == null || !user.IsActive)
            return Unauthorized(new { message = "Usuário inválido ou inativo." });

        return Ok(new LoginResponse
        {
            Token = _tokenService.GenerateToken(user),
            RefreshToken = _tokenService.GenerateRefreshToken(user),
            User = new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            }
        });
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
    public async Task<IActionResult> Logout([FromBody] LogoutRequest? request)
    {
        var token = Request.Headers.Authorization.FirstOrDefault()?.Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token))
            return BadRequest(new { message = "Token não fornecido." });

        await _logoutUseCase.Execute(User, token, request?.RefreshToken);
        return Ok(new { message = "Logout realizado com sucesso." });
    }
}
