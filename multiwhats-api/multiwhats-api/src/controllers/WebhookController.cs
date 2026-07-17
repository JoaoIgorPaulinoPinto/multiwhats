using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using multiwhats_api.src.data.dtos.Webhook;
using multiwhats_api.src.usecases.interfaces.MensagemInterfaces;

[ApiController]
[Route("api/[controller]")]
public class WebhookController : ControllerBase
{
    private readonly IHubContext<WhatsappHub> _hubContext;
    private readonly ISalvarMensagemRecebidaUseCase _salvarMensagemRecebidaUseCase;

    public WebhookController(IHubContext<WhatsappHub> hubContext, ISalvarMensagemRecebidaUseCase salvarMensagemRecebidaUseCase)
    {
        _hubContext = hubContext;
        _salvarMensagemRecebidaUseCase = salvarMensagemRecebidaUseCase;
    }

    [HttpPost("whatsapp")]
    public async Task<IActionResult> ReceiveMessage([FromBody] WhatsappMessageDto payload)
    {
        await _hubContext.Clients.All.SendAsync("ReceberNovaMensagem", payload);
        await _salvarMensagemRecebidaUseCase.Execute(payload);
        return Ok(new { message = "Notificação enviada para a Web!" });
    }
}