using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.usecases.interfaces.DeviceInterfaces;

[ApiController]
[Route("api/device")]
public class DeviceController : ControllerBase
{
    private readonly ISaveDeviceUseCase _saveDeviceUseCase;
    private readonly IDeviceRepository _deviceRepository;

    public DeviceController(
        ISaveDeviceUseCase saveDeviceUseCase,
        IDeviceRepository deviceRepository)
    {
        _saveDeviceUseCase = saveDeviceUseCase;
        _deviceRepository = deviceRepository;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> SaveDevice([FromBody] DeviceRequest request)
    {
        var device = await _saveDeviceUseCase.Execute(request);

        Console.WriteLine($"[Device] Device info saved: Jid={device.Jid}, Phone={device.PhoneNumber}, PushName={device.PushName}, Platform={device.Platform}");

        return Ok(new
        {
            device.Id,
            device.Jid,
            device.PhoneNumber,
            device.PushName,
            device.Platform,
            device.ConnectedAt
        });
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetCurrentDevice()
    {
        var device = await _deviceRepository.GetCurrentAsync();
        if (device == null)
            return NotFound(new { message = "Nenhum dispositivo conectado." });

        return Ok(new
        {
            device.Id,
            device.Jid,
            device.PhoneNumber,
            device.PushName,
            device.Platform,
            device.ConnectedAt,
            device.UpdatedAt
        });
    }
}
