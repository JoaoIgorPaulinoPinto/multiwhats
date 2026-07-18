using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.usecases.interfaces.ContatoInterfaces;
using System.Security.Claims;

namespace multiwhats_api.src.controllers;

[ApiController]
[Route("api/contato")]
public class ContatoController : ControllerBase
{
    private readonly ICriarContatoUseCase _criarContatoUseCase;
    private readonly IPegarContatos _pegarContatoUseCases;
    private readonly IDeletarContatoUseCase _deletarContatoUseCase;
    private string? UsuarioId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    public ContatoController(ICriarContatoUseCase criarContatoUseCase, IPegarContatos pegarCOntatos, IDeletarContatoUseCase deletarContatoUseCase)
    {
        _criarContatoUseCase = criarContatoUseCase;
        _deletarContatoUseCase = deletarContatoUseCase;
        _pegarContatoUseCases = pegarCOntatos;
    }
    [HttpPost("/criar")]
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
    [HttpGet("/pegar/todos")]
    [Authorize]
    public async Task<IActionResult> PegarTodosContatos()
    {
        try
        {
            if (UsuarioId == null) throw new UnauthorizedAccessException();
            var contato = await _pegarContatoUseCases.Execute(int.Parse(UsuarioId));
            return Created("", new { message = "Contatos listados com sucesso.", contato });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
    [HttpGet("/pegar/{id}")]
    [Authorize]
    public async Task<IActionResult> PegarTodosContatos([FromBody] int contatoId)
    {
        try
        {
            if (UsuarioId == null) throw new UnauthorizedAccessException();
            var contato = await _pegarContatoUseCases.Execute(contatoId, int.Parse(UsuarioId));
            return Created("", new { message = "Contato encontrado.", contato });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
    [HttpDelete("/delete/{id}")]
    [Authorize]
    public async Task<IActionResult> DeletarContato([FromQuery] int contatoId)
    {
        try
        {
            if (UsuarioId == null) throw new UnauthorizedAccessException();
            var contato = await _deletarContatoUseCase.Execute(contatoId, int.Parse(UsuarioId));
            return Created("", new { message = "Contato deletado.", contato });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

}
