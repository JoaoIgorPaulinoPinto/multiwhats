using Microsoft.EntityFrameworkCore;
using multiwhats_api.src.data.db;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;

namespace multiwhats_api.src.repositories.repositories;

public class DeviceRepository : IDeviceRepository
{
    private readonly AppDbContext _context;
    private readonly ILegacyDbSyncService _legacyDb;
    private readonly ILogger<DeviceRepository> _logger;

    public DeviceRepository(AppDbContext context, ILegacyDbSyncService legacyDb, ILogger<DeviceRepository> logger)
    {
        _context = context;
        _legacyDb = legacyDb;
        _logger = logger;
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

        _ = Task.Run(async () =>
        {
            try { await _legacyDb.SyncDeviceAsync(existing ?? device); }
            catch (Exception ex) { _logger.LogError(ex, "Erro ao sincronizar dispositivo com LegacyDB"); }
        });

        return existing ?? device;
    }
}
