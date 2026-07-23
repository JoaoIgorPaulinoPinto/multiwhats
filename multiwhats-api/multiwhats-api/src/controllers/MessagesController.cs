using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.usecases.interfaces.MessageInterfaces;
using System.Security.Claims;

namespace multiwhats_api.src.controllers;

/// <summary>
/// CONTROLLER DE MENSAGENS.
/// 
/// Endpoints: /api/messages/*
/// 
/// RESPONSABILIDADES:
/// - POST /api/messages/send: Enviar mensagem WhatsApp
/// - GET /api/messages: Listar todas as mensagens
/// - GET /api/messages/{id}: Detalhar uma mensagem
/// - GET /api/messages/phone/{phoneNumber}: Mensagens por telefone
/// 
/// PECULIARIDADE DO ENDPOINT DE ENVIO:
/// - Tem limite de 100MB ([RequestSizeLimit(100 * 1024 * 1024)])
/// - Isso é necessário porque mensagens com mídia (imagens, vídeos) podem ser grandes
/// - Uma imagem em base64 pode ter vários MB
/// </summary>
[ApiController]
[Route("api/messages")]
public class MessagesController : ControllerBase
{
    private readonly ISendMessageUseCase _sendMessageUseCase;
    private readonly IGetMessagesUseCase _getMessagesUseCase;

    /// <summary>
    /// Propriedade que extrai o ID do usuário do token JWT.
    /// 
    /// COMO FUNCIONA:
    /// - O token JWT contém um claim "NameIdentifier" com o ID do usuário
    /// - User.FindFirst(ClaimTypes.NameIdentifier) extrai esse valor
    /// - int.Parse() converte para número
    /// 
    /// EXEMPLO: Se o JWT diz "NameIdentifier: 1", UserId retorna 1
    /// </summary>
    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    public MessagesController(
        ISendMessageUseCase sendMessageUseCase,
        IGetMessagesUseCase getMessagesUseCase)
    {
        _sendMessageUseCase = sendMessageUseCase;
        _getMessagesUseCase = getMessagesUseCase;
    }

    /// <summary>
    /// Envia uma mensagem WhatsApp.
    /// 
    /// POST /api/messages/send
    /// Headers: Authorization: Bearer {token}
    /// Body: { "jid": "5511999999999@c.us", "text": "Olá", "type": "Text" }
    /// Response: { "message": "Mensagem enviada com sucesso" }
    /// 
    /// FLUXO:
    /// 1. Recebe o request (JID, texto, tipo, mídia)
    /// 2. Envia para o SendMessageUseCase
    /// 3. O Use Case: Strategy → Node.js → Banco → SignalR
    /// 4. Retorna sucesso ou falha
    /// 
    /// NOTA: Tem limite de 100MB para aceitar mensagens com mídia grande
    /// </summary>
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

    /// <summary>
    /// Lista todas as mensagens do sistema.
    /// 
    /// GET /api/messages
    /// Response: Lista de MessageSummaryResponse
    /// </summary>
    [HttpGet]
    [Authorize]
    [RequestSizeLimit(100 * 1024 * 1024)]
    public async Task<IActionResult> GetAll()
    {
        var messages = await _getMessagesUseCase.ExecuteAll();
        return Ok(messages);
    }

    /// <summary>
    /// Detalha uma mensagem específica.
    /// 
    /// GET /api/messages/5
    /// Response: MessageDetailResponse ou 404 NotFound
    /// </summary>
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

    /// <summary>
    /// Lista todas as mensagens de um número de telefone específico.
    /// 
    /// GET /api/messages/phone/5511999999999
    /// 
    /// Útil para encontrar todas as conversas com um contato específico.
    /// O número pode ter formatação (ex: "(11) 99999-9999") - o sistema remove automaticamente.
    /// </summary>
    [HttpGet("phone/{phoneNumber}")]
    [Authorize]
    [RequestSizeLimit(100 * 1024 * 1024)]
    public async Task<IActionResult> GetByPhoneNumber(string phoneNumber)
    {
        var messages = await _getMessagesUseCase.ExecuteByPhoneNumber(phoneNumber);
        return Ok(messages);
    }
}
