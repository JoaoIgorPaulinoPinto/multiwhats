using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.usecases.interfaces.ContatoInterfaces;
using System.Security.Claims;

namespace multiwhats_api.src.controllers;

[ApiController]
[Route("api/[controller]")]
public class ContatoController : ControllerBase
{
    private readonly ICriarContatoUseCase _criarContatoUseCase;
    private string? UsuarioId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    public ContatoController(ICriarContatoUseCase criarContatoUseCase)
    {
        _criarContatoUseCase = criarContatoUseCase;
    }
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CriarContato([FromBody] CriarContatoRequest request)
    {
        try
        {
            if (UsuarioId == null) throw new UnauthorizedAccessException();

            var contato = await _criarContatoUseCase.Execute(request, int.Parse(UsuarioId));
            return Created("", new { message = "Contato criado com sucesso.", contato });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> PegarTodosContatos([FromBody] CriarContatoRequest request)
    {
        try
        {
            if (UsuarioId == null) throw new UnauthorizedAccessException();
            var contato = await _criarContatoUseCase.Execute(request, int.Parse(UsuarioId));
            return Created("", new { message = "Contato criado com sucesso.", contato });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
