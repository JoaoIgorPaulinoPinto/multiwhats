using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.usecases.interfaces.ContatoInterfaces;

namespace multiwhats_api.src.usecases.usecases.ContatoUseCases
{
    public class PegarContatoPorNumero : IPegarContatoPorNumeroUseCase
    {
        private readonly IContatoRepository _contatoRepository;

        public PegarContatoPorNumero(IContatoRepository contatoRepository)
        {
            _contatoRepository = contatoRepository;
        }

        public async Task<ContatoResponse?> Execute(string numero)
        {
            if (string.IsNullOrEmpty(numero))
            {
                throw new ArgumentException("Número vazio");
            }
            Contato? contato = await _contatoRepository.GetByNumberAsync(numero);
            if (contato == null)
            {
                return null;
            }

            return new ContatoResponse
            {
                Id = contato.Id,
                Nome = contato.Nome,
                Numero = contato.Numero,
                GrupoId = contato.GrupoId,
                OcorrenciaAtualId = contato.OcorrenciaAtualId
            };
        }
    }
}
