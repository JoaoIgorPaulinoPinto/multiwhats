using multiwhats_api.src.data.dtos.Requests;
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

    public async Task<Device> Execute(DeviceRequest request)
    {
        var device = new Device
        {
            Jid = request.Jid,
            PhoneNumber = request.PhoneNumber,
            PushName = request.PushName,
            Platform = request.Platform
        };

        return await _deviceRepository.SaveAsync(device);
    }
}
