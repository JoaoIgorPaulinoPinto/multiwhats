using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using multiwhats_api.src.data.dtos.Webhook;
using multiwhats_api.src.usecases.interfaces.MensagemInterfaces;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class WebhookController : ControllerBase
{
    private readonly IHubContext<WhatsappHub> _hubContext;
    private readonly ISalvarMensagemRecebidaUseCase _salvarMensagemRecebidaUseCase;
    private string? UsuarioId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    public WebhookController(IHubContext<WhatsappHub> hubContext, ISalvarMensagemRecebidaUseCase salvarMensagemRecebidaUseCase)
    {
        _hubContext = hubContext;
        _salvarMensagemRecebidaUseCase = salvarMensagemRecebidaUseCase;
    }

    [HttpPost("whatsapp")]
    [AllowAnonymous]
    public async Task<IActionResult> ReceiveMessage([FromBody] WhatsappMessageDto payload)
    {
        
        await _hubContext.Clients.All.SendAsync("ReceberNovaMensagem", payload);
        await _salvarMensagemRecebidaUseCase.Execute(payload);
        return Ok(new { message = "Notificação enviada para a Web!" });
    }
}