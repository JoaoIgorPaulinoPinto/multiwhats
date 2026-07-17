using Microsoft.EntityFrameworkCore;
using multiwhats_api.src.data.db;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;

namespace multiwhats_api.src.repositories.repositories;

public class GrupoRepository : IGrupoRepository
{
    private readonly AppDbContext _context;

    public GrupoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Grupo>> GetAllAsync()
    {
        return await _context.Grupos.AsNoTracking().ToListAsync();
    }

    public async Task<Grupo?> GetByIdAsync(int id)
    {
        return await _context.Grupos
            .AsNoTracking()
            .Include(g => g.Membros)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<Grupo> AddAsync(Grupo grupo)
    {
        _context.Grupos.Add(grupo);
        await _context.SaveChangesAsync();
        return grupo;
    }

    public async Task<Grupo> UpdateAsync(Grupo grupo)
    {
        _context.Entry(grupo).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return grupo;
    }

    public async Task DeleteAsync(int id)
    {
        var grupo = await _context.Grupos.FindAsync(id);
        if (grupo is not null)
        {
            _context.Grupos.Remove(grupo);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Contato>> GetMembrosAsync(int grupoId)
    {
        return await _context.Contatos
            .AsNoTracking()
            .Where(c => c.GrupoId == grupoId)
            .ToListAsync();
    }
}
