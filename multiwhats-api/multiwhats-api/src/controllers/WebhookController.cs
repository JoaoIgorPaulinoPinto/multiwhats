using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using multiwhats_api.src.data.dtos.Webhook;
using multiwhats_api.src.usecases.interfaces.MessageInterfaces;

/// <summary>
/// CONTROLLER DE WEBHOOK - Ponto de entrada para mensagens do WhatsApp.
/// 
/// Endpoints: /api/webhook/*
/// 
/// O QUE É UM WEBHOOK:
/// - É um "ponto de escuta" que recebe dados de um serviço externo
/// - Quando uma mensagem chega no WhatsApp, o Node.js envia um POST para este endpoint
/// - O ASP.NET recebe, processa e salva no banco
/// 
/// FLUXO COMPLETO:
/// 1. Cliente envia mensagem no WhatsApp
/// 2. WhatsApp Web.js (no Node.js) captura a mensagem
/// 3. Node.js envia POST para http://localhost:5261/api/webhook/whatsapp
/// 4. Este controller recebe o payload
/// 5. Delega para o SaveIncomingMessageUseCase
/// 6. O Use Case salva no banco e notifica o Frontend via SignalR
/// 
/// PECULIARIDADE: [AllowAnonymous]
/// - Este endpoint NÃO exige autenticação!
/// - Isso é necessário porque o Node.js não tem token JWT
/// - O Node.js é um serviço interno (localhost), então não precisa de autenticação
/// 
/// SEGURANÇA:
/// - Em produção, este endpoint deveria estar protegido por IP whitelist
/// - Ou usar um token de serviço (não de usuário)
/// - Por enquanto, é acessível de qualquer lugar (aceitável para desenvolvimento)
/// </summary>
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

    /// <summary>
    /// Recebe mensagens do WhatsApp via webhook.
    /// 
    /// POST /api/webhook/whatsapp
    /// Body: WhatsAppWebhookDto (from, phoneNumber, body, timestamp, messageType, etc.)
    /// Response: { "message": "Notificação enviada para a Web!" }
    /// 
    /// FLUXO DETALHADO:
    /// 1. Recebe o payload do Node.js
    /// 2. Loga no console (para debug): quem enviou, tipo, se tem mídia
    /// 3. Chama o SaveIncomingMessageUseCase para processar
    /// 4. Retorna sucesso ou erro detalhado
    /// 
    /// TRATAMENTO DE ERROS:
    /// - Se der erro, retorna 500 com detalhes completos
    /// - Inclui: tipo do erro, mensagem, stack trace, e erro interno
    /// - Isso facilita muito o debug (ver exatamente o que deu errado)
    /// </summary>
    [HttpPost("whatsapp")]
    [AllowAnonymous]  // NÃO exige autenticação (o Node.js não tem token)

    public async Task<IActionResult> ReceiveMessage([FromBody] WhatsAppWebhookDto payload)
    {
        try
        {
            // Log resumido no console (evita poluir com mensagens muito longas)
            var bodyPreview = payload.Body?.Length > 50 ? payload.Body.Substring(0, 50) + "..." : payload.Body;
            Console.WriteLine($"[Webhook] Recebida msg de {payload.From} tipo={payload.MessageType} hasMedia={payload.HasMedia} msgId={payload.MessageId} body={bodyPreview}");

            // Delega para o Use Case processar (deduplicação, salvar, notificar)
            await _saveIncomingMessageUseCase.Execute(payload);
            return Ok(new { message = "Notificação enviada para a Web!" });
        }
        catch (Exception ex)
        {
            // ── TRATAMENTO DE ERRO DETALHADO ──
            // Mostra erro completo no console para facilitar debug
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Webhook] ERRO: {ex.GetType().Name}: {ex.Message}");
            Console.WriteLine($"[Webhook] StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[Webhook] Inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
            }
            Console.ResetColor();

            // Retorna 500 com detalhes do erro (útil para debug, mas CUIDADO em produção!)
            return StatusCode(500, new { error = ex.Message, type = ex.GetType().Name });
        }
    }
}
