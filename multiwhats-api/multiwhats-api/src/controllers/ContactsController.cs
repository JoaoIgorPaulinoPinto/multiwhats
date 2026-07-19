using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.usecases.interfaces.ContactInterfaces;
using System.Security.Claims;

namespace multiwhats_api.src.controllers;

[ApiController]
[Route("api/contacts")]
[Authorize]
public class ContactsController : ControllerBase
{
    private readonly ICreateContactUseCase _createContactUseCase;
    private readonly IGetContactsUseCase _getContactsUseCase;
    private readonly IDeleteContactUseCase _deleteContactUseCase;
    private readonly IAssignContactUseCase _assignContactUseCase;

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    public ContactsController(
        ICreateContactUseCase createContactUseCase,
        IGetContactsUseCase getContactsUseCase,
        IDeleteContactUseCase deleteContactUseCase,
        IAssignContactUseCase assignContactUseCase)
    {
        _createContactUseCase = createContactUseCase;
        _getContactsUseCase = getContactsUseCase;
        _deleteContactUseCase = deleteContactUseCase;
        _assignContactUseCase = assignContactUseCase;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateContactRequest request)
    {
        try
        {
            var contact = await _createContactUseCase.Execute(request, UserId);
            return Created("", new { message = "Contato criado com sucesso.", contact });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var contacts = await _getContactsUseCase.ExecuteAll();
        return Ok(contacts);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var contact = await _getContactsUseCase.ExecuteById(id);
        if (contact == null)
            return NotFound(new { message = "Contato não encontrado." });
        return Ok(contact);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _deleteContactUseCase.Execute(id, UserId);
            return Ok(new { message = "Contato deletado." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/assign")]
    public async Task<IActionResult> Assign(int id, [FromBody] AssignContactRequest request)
    {
        try
        {
            var contact = await _assignContactUseCase.Assign(id, request.ClientId);
            return Ok(new { message = "Contato atribuído ao cliente.", contact });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/unassign")]
    public async Task<IActionResult> Unassign(int id)
    {
        try
        {
            var contact = await _assignContactUseCase.Unassign(id);
            return Ok(new { message = "Contato desatrelado do cliente.", contact });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

}
