namespace multiwhats_api.src.data.dtos
{
    public record EnviarMensagemRequest
    {
        public string conteudo { get; set; } = "";
        public int destinatarioId { get; set; } = 0;
    }
}
