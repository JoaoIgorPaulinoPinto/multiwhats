using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using multiwhats_api.src.data.dtos.Webhook;
using multiwhats_api.src.usecases.interfaces.MessageInterfaces;

// Endpoints: /api/webhook/* (POST whatsapp - recebe mensagens do WhatsApp via Node.js)
[ApiController]
[Route("api/webhook")]
public class WebhookController : ControllerBase
{
    private readonly IHubContext<WhatsappHub> _hubContext;
    private readonly ISaveIncomingMessageUseCase _saveIncomingMessageUseCase;
    private readonly IUpdateMessageDeliveryStatusUseCase _updateMessageDeliveryStatusUseCase;

    public WebhookController(
        IHubContext<WhatsappHub> hubContext,
        ISaveIncomingMessageUseCase saveIncomingMessageUseCase,
        IUpdateMessageDeliveryStatusUseCase updateMessageDeliveryStatusUseCase)
    {
        _hubContext = hubContext;
        _saveIncomingMessageUseCase = saveIncomingMessageUseCase;
        _updateMessageDeliveryStatusUseCase = updateMessageDeliveryStatusUseCase;
    }

    // POST /api/webhook/whatsapp - Receber mensagens do WhatsApp via webhook
    [HttpPost("whatsapp")]
    [AllowAnonymous]

    public async Task<IActionResult> ReceiveMessage([FromBody] WhatsAppWebhookDto payload)
    {
        try
        {
            var bodyPreview = payload.Body?.Length > 50 ? payload.Body.Substring(0, 50) + "..." : payload.Body;
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

    // POST /api/webhook/status - Recebe atualização de status de entrega (ack)
    // das mensagens enviadas, reportada pelo Node (message_ack).
    [HttpPost("status")]
    [AllowAnonymous]
    public async Task<IActionResult> ReceiveDeliveryStatus([FromBody] MessageDeliveryStatusDto payload)
    {
        try
        {
            var updated = await _updateMessageDeliveryStatusUseCase.Execute(payload);
            return Ok(new { message = updated != null ? "Status atualizado" : "Mensagem não encontrada" });
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Webhook] ERRO status: {ex.GetType().Name}: {ex.Message}");
            Console.ResetColor();
            return StatusCode(500, new { error = ex.Message, type = ex.GetType().Name });
        }
    }
}
