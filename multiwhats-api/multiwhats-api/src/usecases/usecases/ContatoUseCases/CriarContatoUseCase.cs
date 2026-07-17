using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.usecases.interfaces.ContatoInterfaces;

namespace multiwhats_api.src.usecases.usecases.ContatoUseCases
{
    public class CriarContatoUseCase : ICriarContatoUseCase
    {
        private readonly IContatoRepository _contatoRepository;

        public CriarContatoUseCase(IContatoRepository contatoRepository)
        {
            _contatoRepository = contatoRepository;
        }
        public async Task<int> Execute(CriarContatoRequest dto)
        {

            //adicionar validações de regra de negócio aqui
            Contato contato = new Contato(
                dto.Nome,
                dto.Numero,
                dto.GrupoId,
                dto.OcorrenciaAtualId
            );
            contato = await _contatoRepository.AddAsync(contato);
            return contato.Id;

        }
    }
}
