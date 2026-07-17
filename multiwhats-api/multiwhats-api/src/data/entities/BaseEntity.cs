namespace multiwhats_api.src.data.entities;

public abstract class BaseEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
}