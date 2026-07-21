using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.entities;

namespace multiwhats_api.src.usecases.interfaces.DeviceInterfaces;

public interface ISaveDeviceUseCase
{
    Task<Device> Execute(DeviceRequest request);
}
