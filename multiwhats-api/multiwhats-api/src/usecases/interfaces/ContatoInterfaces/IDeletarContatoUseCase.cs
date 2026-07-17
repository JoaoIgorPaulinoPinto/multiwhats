namespace multiwhats_api.src.usecases.interfaces.ContatoInterfaces
{
    public interface IDeletarContatoUseCase
    {
        public Task<bool> Execute(int contatoId, int UsuarioId);
    }
}
