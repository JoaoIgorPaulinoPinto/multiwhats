using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.usecases.interfaces.DeviceInterfaces;

namespace multiwhats_api.src.usecases.usecases.DeviceUseCases;

public class SaveDeviceUseCase : ISaveDeviceUseCase
{
    private readonly IDeviceRepository _deviceRepository;

    public SaveDeviceUseCase(IDeviceRepository deviceRepository)
    {
        _deviceRepository = deviceRepository;
    }

    public async Task<DeviceResponse> Execute(DeviceRequest request)
    {
        var device = new Device
        {
            Jid = request.Jid,
            PhoneNumber = request.PhoneNumber,
            PushName = request.PushName,
            Platform = request.Platform
        };

        var saved = await _deviceRepository.SaveAsync(device);

        return new DeviceResponse
        {
            Id = saved.Id,
            Jid = saved.Jid,
            PhoneNumber = saved.PhoneNumber,
            PushName = saved.PushName,
            Platform = saved.Platform,
            ConnectedAt = saved.ConnectedAt,
            UpdatedAt = saved.UpdatedAt
        };
    }
}
