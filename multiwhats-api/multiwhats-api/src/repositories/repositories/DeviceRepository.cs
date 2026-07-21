using Microsoft.EntityFrameworkCore;
using multiwhats_api.src.data.db;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;

namespace multiwhats_api.src.repositories.repositories;

public class DeviceRepository : IDeviceRepository
{
    private readonly AppDbContext _context;

    public DeviceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Device?> GetCurrentAsync()
    {
        return await _context.Devices
            .AsNoTracking()
            .OrderByDescending(d => d.UpdatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<Device> SaveAsync(Device device)
    {
        var existing = await _context.Devices
            .OrderByDescending(d => d.UpdatedAt)
            .FirstOrDefaultAsync();

        if (existing != null)
        {
            existing.Jid = device.Jid;
            existing.PhoneNumber = device.PhoneNumber;
            existing.PushName = device.PushName;
            existing.Platform = device.Platform;
            existing.UpdatedAt = DateTime.UtcNow;
            _context.Entry(existing).State = EntityState.Modified;
        }
        else
        {
            device.ConnectedAt = DateTime.UtcNow;
            device.UpdatedAt = DateTime.UtcNow;
            _context.Devices.Add(device);
        }

        await _context.SaveChangesAsync();
        return existing ?? device;
    }
}
