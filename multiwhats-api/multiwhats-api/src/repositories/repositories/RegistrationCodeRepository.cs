using Microsoft.EntityFrameworkCore;
using multiwhats_api.src.data.db;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;

namespace multiwhats_api.src.repositories.repositories;

public class RegistrationCodeRepository : IRegistrationCodeRepository
{
    private readonly AppDbContext _context;

    public RegistrationCodeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RegistrationCode?> GetByCodeAsync(string code)
    {
        return await _context.Set<RegistrationCode>()
            .AsNoTracking()
            .FirstOrDefaultAsync(rc => rc.Code == code);
    }

    public async Task<bool> ExistsAsync(string code)
    {
        return await _context.Set<RegistrationCode>()
            .AsNoTracking()
            .AnyAsync(rc => rc.Code == code);
    }

    public async Task<RegistrationCode?> GetTrackedByCodeAsync(string code)
    {
        return await _context.Set<RegistrationCode>()
            .FirstOrDefaultAsync(rc => rc.Code == code);
    }

    public async Task<RegistrationCode> AddAsync(RegistrationCode registrationCode)
    {
        _context.Set<RegistrationCode>().Add(registrationCode);
        await _context.SaveChangesAsync();
        return registrationCode;
    }

    public async Task<RegistrationCode> UpdateAsync(RegistrationCode registrationCode)
    {
        _context.Entry(registrationCode).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return registrationCode;
    }

    public async Task<List<RegistrationCode>> GetByCreatorAsync(int userId)
    {
        return await _context.Set<RegistrationCode>()
            .AsNoTracking()
            .Where(rc => rc.CreatedByUserId == userId)
            .OrderByDescending(rc => rc.CreatedAt)
            .ToListAsync();
    }
}
