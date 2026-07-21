using multiwhats_api.src.data.entities;

namespace multiwhats_api.src.repositories.interfaces;

public interface IDeviceRepository
{
    Task<Device?> GetCurrentAsync();
    Task<Device> SaveAsync(Device device);
}
