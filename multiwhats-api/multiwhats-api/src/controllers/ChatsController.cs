using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.usecases.interfaces.ChatInterfaces;
using multiwhats_api.src.usecases.interfaces.MessageInterfaces;
using multiwhats_api.src.usecases.interfaces.OccurrenceInterfaces;

namespace multiwhats_api.src.controllers;

/// <summary>
/// CONTROLLER DE CONVERSAS (CHATS).
/// 
/// Endpoints: /api/chats/*
/// 
/// RESPONSABILIDADES:
/// - GET /api/chats: Listar conversas (paginado)
/// - GET /api/chats/{id}: Detalhar uma conversa
/// - GET /api/chats/{id}/messages: Mensagens de uma conversa (paginado)
/// - GET /api/chats/{id}/occurrences: Ocorrências de uma conversa
/// 
/// NOTA: Todos os endpoints exigem autenticação ([Authorize] no controller)
/// </summary>
[ApiController]
[Route("api/chats")]
[Authorize]  // TODOS os endpoints deste controller exigem login
public class ChatsController : ControllerBase
{
    private readonly IGetChatsUseCase _getChatsUseCase;
    private readonly ICreateChatUseCase _createChatUseCase;
    private readonly IGetMessagesUseCase _getMessagesUseCase;
    private readonly IGetOccurrencesUseCase _getOccurrencesUseCase;

    public ChatsController(
        IGetChatsUseCase getChatsUseCase,
        ICreateChatUseCase createChatUseCase,
        IGetMessagesUseCase getMessagesUseCase,
        IGetOccurrencesUseCase getOccurrencesUseCase)
    {
        _getChatsUseCase = getChatsUseCase;
        _createChatUseCase = createChatUseCase;
        _getMessagesUseCase = getMessagesUseCase;
        _getOccurrencesUseCase = getOccurrencesUseCase;
    }

    /// <summary>
    /// Lista todas as conversas com paginação.
    /// 
    /// GET /api/chats?page=1&pageSize=20
    /// 
    /// Parâmetros de query string:
    /// - page: número da página (padrão: 1)
    /// - pageSize: itens por página (padrão: 20)
    /// 
    /// Response: PaginatedResponse com items, totalCount, page, pageSize, totalPages
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var chats = await _getChatsUseCase.ExecuteAll(page, pageSize);
        return Ok(chats);
    }

    /// <summary>
    /// Detalha uma conversa específica, incluindo suas ocorrências.
    /// 
    /// GET /api/chats/5
    /// Response: ChatDetailResponse com dados completos + lista de ocorrências
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var chat = await _getChatsUseCase.ExecuteById(id);
        if (chat == null)
            return NotFound(new { message = "Chat não encontrado." });
        return Ok(chat);
    }

    /// <summary>
    /// Lista as mensagens de uma conversa com paginação.
    /// 
    /// GET /api/chats/5/messages?page=1&pageSize=50
    /// 
    /// As mensagens são retornadas em ordem cronológica (mais antiga primeiro).
    /// O padrão são 50 mensagens por página (mais que os chats, pois conversas são longas).
    /// </summary>
    [HttpGet("{id}/messages")]
    public async Task<IActionResult> GetMessages(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var messages = await _getMessagesUseCase.ExecuteByChat(id, page, pageSize);
        return Ok(messages);
    }

    /// <summary>
    /// Lista todas as ocorrências (chamados) de uma conversa.
    /// 
    /// GET /api/chats/5/occurrences
    /// Response: Lista de OccurrenceDetailResponse
    /// </summary>
    [HttpGet("{id}/occurrences")]
    public async Task<IActionResult> GetOccurrences(int id)
    {
        var occurrences = await _getOccurrencesUseCase.ExecuteByChat(id);
        return Ok(occurrences);
    }
}
