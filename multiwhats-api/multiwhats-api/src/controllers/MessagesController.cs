using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.usecases.interfaces.MessageInterfaces;
using System.Security.Claims;

namespace multiwhats_api.src.controllers;

// Endpoints: /api/messages/* (POST send, GET list, detail, by phone)
[ApiController]
[Route("api/messages")]
public class MessagesController : ControllerBase
{
    private readonly ISendMessageUseCase _sendMessageUseCase;
    private readonly IGetMessagesUseCase _getMessagesUseCase;

    // Extrai o ID do usuário do token JWT
    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    public MessagesController(
        ISendMessageUseCase sendMessageUseCase,
        IGetMessagesUseCase getMessagesUseCase)
    {
        _sendMessageUseCase = sendMessageUseCase;
        _getMessagesUseCase = getMessagesUseCase;
    }

    // POST /api/messages/send - Enviar mensagem WhatsApp (limite 100MB para mídia)
    [HttpPost("send")]
    [Authorize]
    [RequestSizeLimit(100 * 1024 * 1024)] // 100 MB - necessário para mensagens com mídia
    public async Task<IActionResult> Send([FromBody] SendMessageRequest request)
    {
        var result = await _sendMessageUseCase.Execute(request, UserId);
        if (result)
            return Ok(new { message = "Mensagem enviada com sucesso" });

        return BadRequest(new { message = "Falha ao enviar mensagem" });
    }

    // GET /api/messages - Listar todas as mensagens
    [HttpGet]
    [Authorize]
    [RequestSizeLimit(100 * 1024 * 1024)]
    public async Task<IActionResult> GetAll()
    {
        var messages = await _getMessagesUseCase.ExecuteAll();
        return Ok(messages);
    }

    // GET /api/messages/{id} - Detalhar mensagem
    [HttpGet("{id}")]
    [Authorize]
    [RequestSizeLimit(100 * 1024 * 1024)]
    public async Task<IActionResult> GetById(int id)
    {
        var message = await _getMessagesUseCase.ExecuteById(id);
        if (message == null)
            return NotFound(new { message = "Mensagem não encontrada." });
        return Ok(message);
    }

    // GET /api/messages/phone/{phoneNumber} - Mensagens por telefone
    [HttpGet("phone/{phoneNumber}")]
    [Authorize]
    [RequestSizeLimit(100 * 1024 * 1024)]
    public async Task<IActionResult> GetByPhoneNumber(string phoneNumber)
    {
        var messages = await _getMessagesUseCase.ExecuteByPhoneNumber(phoneNumber);
        return Ok(messages);
    }
}
