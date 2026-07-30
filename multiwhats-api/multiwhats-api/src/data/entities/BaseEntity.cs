using System.ComponentModel.DataAnnotations.Schema;

namespace multiwhats_api.src.data.entities;

public abstract class BaseEntity
{
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Column("last_update")]
    public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
    [Column("is_deleted")]
    public bool IsDeleted { get; set; }
    [Column("created_by_user_id")]
    public int? CreatedByUserId { get; set; }
    [Column("last_updated_by_user_id")]
    public int? LastUpdatedByUserId { get; set; }
}
