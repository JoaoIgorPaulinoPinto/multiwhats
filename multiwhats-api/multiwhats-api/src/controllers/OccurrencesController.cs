using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.usecases.interfaces.OccurrenceInterfaces;
using System.Security.Claims;

namespace multiwhats_api.src.controllers;

[ApiController]
[Route("api/occurrences")]
[Authorize]
public class OccurrencesController : ControllerBase
{
    private readonly ICreateOccurrenceUseCase _createOccurrenceUseCase;
    private readonly IGetOccurrencesUseCase _getOccurrencesUseCase;
    private readonly IUpdateOccurrenceUseCase _updateOccurrenceUseCase;
    private readonly IDeleteOccurrenceUseCase _deleteOccurrenceUseCase;
    private readonly IAdvanceOccurrenceStatusUseCase _advanceOccurrenceStatusUseCase;
    private readonly IGetOccurrenceMetricsUseCase _getOccurrenceMetricsUseCase;

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    public OccurrencesController(
        ICreateOccurrenceUseCase createOccurrenceUseCase,
        IGetOccurrencesUseCase getOccurrencesUseCase,
        IUpdateOccurrenceUseCase updateOccurrenceUseCase,
        IDeleteOccurrenceUseCase deleteOccurrenceUseCase,
        IAdvanceOccurrenceStatusUseCase advanceOccurrenceStatusUseCase,
        IGetOccurrenceMetricsUseCase getOccurrenceMetricsUseCase)
    {
        _createOccurrenceUseCase = createOccurrenceUseCase;
        _getOccurrencesUseCase = getOccurrencesUseCase;
        _updateOccurrenceUseCase = updateOccurrenceUseCase;
        _deleteOccurrenceUseCase = deleteOccurrenceUseCase;
        _advanceOccurrenceStatusUseCase = advanceOccurrenceStatusUseCase;
        _getOccurrenceMetricsUseCase = getOccurrenceMetricsUseCase;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOccurrenceRequest request)
    {
        try
        {
            var occurrence = await _createOccurrenceUseCase.Execute(request, UserId);
            return Created("", new { message = "Ocorrência criada.", occurrence });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var occurrences = await _getOccurrencesUseCase.ExecuteAll();
        return Ok(occurrences);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var occurrence = await _getOccurrencesUseCase.ExecuteById(id);
        if (occurrence == null)
            return NotFound(new { message = "Ocorrência não encontrada." });
        return Ok(occurrence);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOccurrenceRequest request)
    {
        try
        {
            var occurrence = await _updateOccurrenceUseCase.Execute(id, request);
            return Ok(new { message = "Ocorrência atualizada.", occurrence });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _deleteOccurrenceUseCase.Execute(id);
            return Ok(new { message = "Ocorrência deletada." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> AdvanceStatus(int id, [FromBody] AdvanceOccurrenceStatusRequest request)
    {
        try
        {
            var occurrence = await _advanceOccurrenceStatusUseCase.Execute(id, request, UserId);
            var msg = request.Direction == AdvanceDirection.Advance
                ? "Ocorrência avançada para próxima etapa."
                : "Ocorrência retornada para etapa anterior.";
            return Ok(new { message = msg, occurrence });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("metrics")]
    public async Task<IActionResult> GetMetrics()
    {
        var metrics = await _getOccurrenceMetricsUseCase.Execute();
        return Ok(metrics);
    }
}
