using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using multiwhats_api.src.data.dtos;

[ApiController]
[Route("api/[controller]")]
public class WebhookController : ControllerBase
{
    private readonly IHubContext<WhatsappHub> _hubContext;

    public WebhookController(IHubContext<WhatsappHub> hubContext)
    {
        _hubContext = hubContext;
    }

    [HttpPost("whatsapp")]
    public async Task<IActionResult> ReceiveMessage([FromBody] WhatsappMessageDto payload)
    {

        await _hubContext.Clients.All.SendAsync("ReceberNovaMensagem", payload);
        Console.WriteLine($"\n[WHATSAPP RECEBIDO] De: {payload.NotifyName} ({payload.From}) -> {payload.Body}\n");
        return Ok(new { message = "Notificação enviada para a Web!" });
    }
}