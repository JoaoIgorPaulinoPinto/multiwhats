using Microsoft.AspNetCore.Mvc;
using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.usecases.interfaces.MensagemInterfaces;

namespace multiwhats_api.src.controllers;

[ApiController]
[Route("api/[controller]")]
public class MensagensController : ControllerBase
{
    private readonly IEnviarMensagemUseCase _enviarMensagemUseCase;

    public MensagensController(IEnviarMensagemUseCase enviarMensagemUseCase)
    {
        _enviarMensagemUseCase = enviarMensagemUseCase;
    }

    [HttpPost("/send")]

    public async Task<IActionResult> Send([FromBody] EnviarMensagemRequest req)
    {
        var result = await _enviarMensagemUseCase.Execute(req);
        if (result)
            return Ok(new { message = "Mensagem enviada com sucesso" });

        return BadRequest(new { message = "Falha ao enviar mensagem" });
    }
}
