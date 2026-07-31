using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.usecases.interfaces.AuthInterfaces;

namespace multiwhats_api.src.controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IUpdateUserUseCase _updateUserUseCase;

    public UsersController(IUserRepository userRepository, IUpdateUserUseCase updateUserUseCase)
    {
        _userRepository = userRepository;
        _updateUserUseCase = updateUserUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userRepository.GetAllAsync();
        return Ok(users.Select(u => new UserResponse
        {
            Id = u.Id,
            Name = u.Name,
            Role = u.Role.ToString(),
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt
        }).ToList());
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin, Dev")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            var user = await _updateUserUseCase.Execute(id, request);
            return Ok(new { message = "Usuário atualizado com sucesso.", user });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
