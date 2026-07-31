using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.usecases.interfaces.ChatInterfaces;
using multiwhats_api.src.usecases.interfaces.MessageInterfaces;
using multiwhats_api.src.usecases.interfaces.OccurrenceInterfaces;

namespace multiwhats_api.src.controllers;

// Endpoints: /api/chats/* (GET list, detail, messages, occurrences)
[ApiController]
[Route("api/chats")]
[Authorize]
public class ChatsController : ControllerBase
{
    private readonly IGetChatsUseCase _getChatsUseCase;
    private readonly ICreateChatUseCase _createChatUseCase;
    private readonly IGetMessagesUseCase _getMessagesUseCase;
    private readonly IGetOccurrencesUseCase _getOccurrencesUseCase;
    private readonly IMergeChatsUseCase _mergeChatsUseCase;
    private readonly IGetChatFullInfoUseCase _getChatFullInfoUseCase;

    public ChatsController(
        IGetChatsUseCase getChatsUseCase,
        ICreateChatUseCase createChatUseCase,
        IMergeChatsUseCase mergeChatsUseCase,
        IGetMessagesUseCase getMessagesUseCase,
        IGetOccurrencesUseCase getOccurrencesUseCase,
        IGetChatFullInfoUseCase getChatFullInfoUseCase)
    {
        _mergeChatsUseCase = mergeChatsUseCase;
        _getChatsUseCase = getChatsUseCase;
        _createChatUseCase = createChatUseCase;
        _getMessagesUseCase = getMessagesUseCase;
        _getOccurrencesUseCase = getOccurrencesUseCase;
        _getChatFullInfoUseCase = getChatFullInfoUseCase;
    }

    // GET /api/chats - Listar conversas (paginado)
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var chats = await _getChatsUseCase.ExecuteAll(page, pageSize);
        return Ok(chats);
    }

    // GET /api/chats/{id} - Detalhar conversa
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var chat = await _getChatsUseCase.ExecuteById(id);
        if (chat == null)
            return NotFound(new { message = "Chat não encontrado." });
        return Ok(chat);
    }

    // GET /api/chats/{id}/messages - Mensagens da conversa (paginado)
    [HttpGet("{id}/messages")]
    public async Task<IActionResult> GetMessages(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var messages = await _getMessagesUseCase.ExecuteByChat(id, page, pageSize);
        return Ok(messages);
    }

    // GET /api/chats/{id}/occurrences - Ocorrências da conversa
    [HttpGet("{id}/occurrences")]
    public async Task<IActionResult> GetOccurrences(int id)
    {
        var occurrences = await _getOccurrencesUseCase.ExecuteByChat(id);
        return Ok(occurrences);
    }
    // GET /api/chats/{id}/full-info - Informações completas da conversa
    [HttpGet("{id}/full-info")]
    public async Task<IActionResult> GetFullInfo(int id)
    {
        var chat = await _getChatFullInfoUseCase.Execute(id);
        if (chat == null)
            return NotFound(new { message = "Chat não encontrado." });
        return Ok(chat);
    }

    // PATCH /api/chats/merge?mergeJid=xxx&toJid=yyy - Merge duas conversas
    [HttpPatch("merge")]
    public async Task<IActionResult> MergeChats([FromQuery] string mergeJid, [FromQuery] string toJid)
    {
        var merged = await _mergeChatsUseCase.Execute(mergeJid, toJid);
        if (!merged)
            return NotFound(new { message = "Uma ou ambas conversas não encontradas." });

        return Ok(new { message = "Conversas mescladas com sucesso." });
    }
}
