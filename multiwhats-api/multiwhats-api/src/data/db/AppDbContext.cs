using Microsoft.EntityFrameworkCore;
using multiwhats_api.src.data.entities;
namespace multiwhats_api.src.data.db;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    public DbSet<Mensagem> MessageLogs { get; set; }
}