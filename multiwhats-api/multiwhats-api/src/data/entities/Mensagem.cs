using System.ComponentModel.DataAnnotations;

namespace multiwhats_api.src.data.entities
{
    public class Mensagem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string From { get; set; } = string.Empty;

        // Usamos Column(TypeName = "TEXT") para suportar textos longos
        public string Body { get; set; } = string.Empty;

        public long Timestamp { get; set; }

        [StringLength(100)]
        public string? NotifyName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
