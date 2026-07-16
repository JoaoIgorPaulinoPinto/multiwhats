namespace multiwhats_api.src.repositories.interfaces
{
    public interface IMensagemRepository
    {
        public bool SalvarMensagemEnviada(string mensagem, string numero, string idMensagem);
    }
}
