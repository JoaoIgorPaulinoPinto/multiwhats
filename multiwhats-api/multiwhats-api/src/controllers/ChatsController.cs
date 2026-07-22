using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.usecases.interfaces.ChatInterfaces;
using multiwhats_api.src.usecases.interfaces.MessageInterfaces;
using multiwhats_api.src.usecases.interfaces.OccurrenceInterfaces;

namespace multiwhats_api.src.controllers;

[ApiController]
[Route("api/chats")]
[Authorize]
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

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var chats = await _getChatsUseCase.ExecuteAll(page, pageSize);
        return Ok(chats);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var chat = await _getChatsUseCase.ExecuteById(id);
        if (chat == null)
            return NotFound(new { message = "Chat não encontrado." });
        return Ok(chat);
    }

    [HttpGet("{id}/messages")]
    public async Task<IActionResult> GetMessages(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var messages = await _getMessagesUseCase.ExecuteByChat(id, page, pageSize);
        return Ok(messages);
    }

    [HttpGet("{id}/occurrences")]
    public async Task<IActionResult> GetOccurrences(int id)
    {
        var occurrences = await _getOccurrencesUseCase.ExecuteByChat(id);
        return Ok(occurrences);
    }
}
