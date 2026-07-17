using Microsoft.EntityFrameworkCore;
using multiwhats_api.src.data.db;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;

namespace multiwhats_api.src.repositories.repositories;

public class MensagemRepository : IMensagemRepository
{
    private readonly AppDbContext _context;

    public MensagemRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Mensagem>> PegarTodosAsync()
    {
        return await _context.Mensagens
            .AsNoTracking()
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<Mensagem?> PegarPorIdAsync(int id)
    {
        return await _context.Mensagens
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<bool> AdicionarAsync(Mensagem mensagem)
    {
        _context.Mensagens.Add(mensagem);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task DeleteAsync(int id)
    {
        var mensagem = await _context.Mensagens.FindAsync(id);
        if (mensagem is not null)
        {
            _context.Mensagens.Remove(mensagem);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Mensagem>> GetByContatoAsync(int contatoId)
    {
        return await _context.Mensagens
            .AsNoTracking()
            .Where(m => m.ContatoId == contatoId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }
}
