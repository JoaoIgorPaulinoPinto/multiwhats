using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;

namespace multiwhats_api.src.usecases.interfaces.DeviceInterfaces;

public interface ISaveDeviceUseCase
{
    Task<DeviceResponse> Execute(DeviceRequest request);
}
