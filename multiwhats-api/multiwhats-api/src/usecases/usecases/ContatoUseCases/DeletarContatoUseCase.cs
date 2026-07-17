using Microsoft.AspNetCore.Http.HttpResults;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.repositories.repositories;
using multiwhats_api.src.usecases.interfaces.ContatoInterfaces;

namespace multiwhats_api.src.usecases.usecases.ContatoUseCases
{
    public class DeletarContatoUseCase : IDeletarContatoUseCase
    {
        private readonly IContatoRepository _contatoRepository;
        public DeletarContatoUseCase(IContatoRepository contatoRepository)
        {
            _contatoRepository = contatoRepository;
        }

        public async Task<bool> Execute(int contatoId, int usuarioId)
        {
            var contato = await _contatoRepository.GetByIdAsync(contatoId);
            if (contato == null) {
                throw new Exception("Contato não encontrado");
            }
            else
            {
                return await _contatoRepository.DeleteAsync(contatoId);
            }
        }
    }
}
