using System.ComponentModel.DataAnnotations.Schema;

namespace multiwhats_api.src.data.entities
{
    public class Status
    {
        [Column("id")]
        public int Id; 
        [Column("descricao")]
        public string? Descricao { get; private set; }
        [Column("ativo")]
        public bool Ativo { get; private set; }
        public ICollection<Ocorrencia> ocorrencias { get; private set; } = new List<Ocorrencia>();
        public Status() { }

        public Status(int id, string? descricao, bool ativo)
        {
            Id = id;
            Descricao = descricao;
            Ativo = ativo;
        }
    }
}
