using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.usecases.interfaces.ClientInterfaces;
using System.Security.Claims;

namespace multiwhats_api.src.controllers;

[ApiController]
[Route("api/clients")]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly ICreateClientUseCase _createClientUseCase;
    private readonly IGetClientsUseCase _getClientsUseCase;
    private readonly IUpdateClientUseCase _updateClientUseCase;
    private readonly IDeleteClientUseCase _deleteClientUseCase;

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    public ClientsController(
        ICreateClientUseCase createClientUseCase,
        IGetClientsUseCase getClientsUseCase,
        IUpdateClientUseCase updateClientUseCase,
        IDeleteClientUseCase deleteClientUseCase)
    {
        _createClientUseCase = createClientUseCase;
        _getClientsUseCase = getClientsUseCase;
        _updateClientUseCase = updateClientUseCase;
        _deleteClientUseCase = deleteClientUseCase;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClientRequest request)
    {
        try
        {
            var client = await _createClientUseCase.Execute(request, UserId);
            return Created("", new { message = "Cliente criado com sucesso.", client });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var clients = await _getClientsUseCase.ExecuteAll();
        return Ok(clients);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var client = await _getClientsUseCase.ExecuteById(id);
        if (client == null)
            return NotFound(new { message = "Cliente não encontrado." });
        return Ok(client);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClientRequest request)
    {
        try
        {
            var client = await _updateClientUseCase.Execute(id, request);
            return Ok(new { message = "Cliente atualizado.", client });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _deleteClientUseCase.Execute(id);
            return Ok(new { message = "Cliente deletado." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{id}/contacts")]
    public async Task<IActionResult> GetContacts(int id)
    {
        var client = await _getClientsUseCase.ExecuteById(id);
        if (client == null)
            return NotFound(new { message = "Cliente não encontrado." });
        return Ok(client);
    }
}
