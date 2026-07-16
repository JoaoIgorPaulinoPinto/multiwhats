using Microsoft.AspNetCore.Mvc;
using multiwhats_api.src.data.dtos;

namespace multiwhats_api.src.controllers
{
    [ApiController]
    public class MensagensController : ControllerBase
    {

        [HttpPost("/send")]
        public Task<IActionResult> Send([FromBody]EnviarMensagemRequest req)
        {

        }
    }
}
