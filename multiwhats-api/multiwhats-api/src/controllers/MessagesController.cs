using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.usecases.interfaces.MessageInterfaces;
using System.Security.Claims;

namespace multiwhats_api.src.controllers;

[ApiController]
[Route("api/messages")]
public class MessagesController : ControllerBase
{
    private readonly ISendMessageUseCase _sendMessageUseCase;

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    public MessagesController(ISendMessageUseCase sendMessageUseCase)
    {
        _sendMessageUseCase = sendMessageUseCase;
    }

    [HttpPost("send")]
    [Authorize]
    public async Task<IActionResult> Send([FromBody] SendMessageRequest request)
    {
        var result = await _sendMessageUseCase.Execute(request, UserId);
        if (result)
            return Ok(new { message = "Mensagem enviada com sucesso" });

        return BadRequest(new { message = "Falha ao enviar mensagem" });
    }
}
