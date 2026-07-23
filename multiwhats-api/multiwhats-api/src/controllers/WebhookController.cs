using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using multiwhats_api.src.data.dtos.Webhook;
using multiwhats_api.src.usecases.interfaces.MessageInterfaces;

[ApiController]
[Route("api/webhook")]
public class WebhookController : ControllerBase
{
    private readonly IHubContext<WhatsappHub> _hubContext;
    private readonly ISaveIncomingMessageUseCase _saveIncomingMessageUseCase;

    public WebhookController(
        IHubContext<WhatsappHub> hubContext,
        ISaveIncomingMessageUseCase saveIncomingMessageUseCase)
    {
        _hubContext = hubContext;
        _saveIncomingMessageUseCase = saveIncomingMessageUseCase;
    }

    [HttpPost("whatsapp")]
    [AllowAnonymous]

    public async Task<IActionResult> ReceiveMessage([FromBody] WhatsAppWebhookDto payload)
    {
        try
        {
            var bodyPreview = payload.Body?.Length > 50 ? payload.Body.Substring(0, 50) + "..." : payload.Body;
            Console.WriteLine($"[Webhook] Recebida msg de {payload.From} tipo={payload.MessageType} hasMedia={payload.HasMedia} msgId={payload.MessageId} body={bodyPreview}");

            await _saveIncomingMessageUseCase.Execute(payload);
            return Ok(new { message = "Notificação enviada para a Web!" });
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Webhook] ERRO: {ex.GetType().Name}: {ex.Message}");
            Console.WriteLine($"[Webhook] StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[Webhook] Inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
            }
            Console.ResetColor();
            return StatusCode(500, new { error = ex.Message, type = ex.GetType().Name });
        }
    }
}
