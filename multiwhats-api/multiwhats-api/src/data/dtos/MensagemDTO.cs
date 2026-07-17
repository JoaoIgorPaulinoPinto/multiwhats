using multiwhats_api.src.data.entities;

namespace multiwhats_api.src.data.dtos
{
    public record EnviarMensagemRequest
    {
        public string conteudo { get; set; } = "";
        public int destinatarioId { get; set; } = 0;
    }
    public record CriarMensagemDTO
    {
        public string conteudo { get; set; } = "";
        public Contato? remetente { get; set; }
        public DateTime dataEnvio { get; set; } = DateTime.Now;
    }
}
