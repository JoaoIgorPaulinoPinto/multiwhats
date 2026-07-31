using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using System.Security.Claims;

namespace multiwhats_api.src.controllers;

[ApiController]
[Route("api/admin/config")]
//[Authorize(Roles = "Admin, Dev")]
public class SystemConfigController : ControllerBase
{   
    private readonly ISystemParameterRepository _systemParameterRepository;
    private readonly SystemConfigService _systemConfigService;

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    public SystemConfigController(
        ISystemParameterRepository systemParameterRepository,
        SystemConfigService systemConfigService)
    {
        _systemParameterRepository = systemParameterRepository;
        _systemConfigService = systemConfigService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var parameters = await _systemParameterRepository.GetAllAsync();
        var response = parameters.Select(p => new SystemParameterResponse
        {
            Id = p.Id,
            Key = p.Key,
            Value = p.Value,
            Type = p.Type,
            Group = p.Group,
            Description = p.Description,
            IsRequired = p.IsRequired,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
            UpdatedByUserId = p.UpdatedByUserId
        }).ToList();
        return Ok(response);
    }

    [HttpGet("{key}")]
    public async Task<IActionResult> GetByKey(string key)
    {
        var param = await _systemParameterRepository.GetByKeyAsync(key);
        if (param == null)
            return NotFound(new { message = "Parâmetro não encontrado." });

        return Ok(new SystemParameterResponse
        {
            Id = param.Id,
            Key = param.Key,
            Value = param.Value,
            Type = param.Type,
            Group = param.Group,
            Description = param.Description,
            IsRequired = param.IsRequired,
            CreatedAt = param.CreatedAt,
            UpdatedAt = param.UpdatedAt,
            UpdatedByUserId = param.UpdatedByUserId
        });
    }

    [HttpPut("{key}")]
    public async Task<IActionResult> Update(string key, [FromBody] UpdateSystemParameterRequest request)
    {
        var param = await _systemParameterRepository.GetByKeyAsync(key);
        if (param == null)
        {
            var parts = key.Split(':');
            param = new data.entities.SystemParameter(
                key, request.Value, "String",
                parts.Length > 1 ? parts[0] : null
            );
            await _systemParameterRepository.AddAsync(param);
        }
        else
        {
            param.UpdateValue(request.Value, UserId);
            await _systemParameterRepository.UpdateAsync(param);
        }

        _systemConfigService.InvalidateCache();

        return Ok(new { message = "Parâmetro atualizado com sucesso." });
    }

    [HttpPost("reload")]
    public async Task<IActionResult> Reload()
    {
        await _systemConfigService.LoadAsync();
        return Ok(new { message = "Cache recarregado com sucesso." });
    }
}
