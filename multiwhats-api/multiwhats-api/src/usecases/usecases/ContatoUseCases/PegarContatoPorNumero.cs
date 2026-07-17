using Microsoft.AspNetCore.Http.HttpResults;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.usecases.interfaces.ContatoInterfaces;

namespace multiwhats_api.src.usecases.usecases.ContatoUseCases
{
    public class PegarContatoPorNumero : IPegarContatos
    {
        private readonly IContatoRepository _contatoRepository;

        public PegarContatoPorNumero(IContatoRepository contatoRepository)
        {
            _contatoRepository = contatoRepository;
        }

        public async Task<ContatoResponse?> Execute(int userId, string numero)
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


        public async Task<List<ContatoResponse>?> Execute()
        {

            List<Contato>? contatos = await _contatoRepository.GetAllAsync();
            List<ContatoResponse> res = new List<ContatoResponse>();
            foreach (var item in contatos)
            {
                res.Add(new ContatoResponse
                {
                    Id = item.Id,
                    Nome = item.Nome,
                    Numero = item.Numero,
                    GrupoId = item.GrupoId,
                    OcorrenciaAtualId = item.OcorrenciaAtualId
                });
            }
            if (res.Count > 0)
            {
                return res;
            }
            else
            {
                return null;
            }
        }
        public async Task<ContatoResponse?> Execute(int contatoId)
        {

            Contato? contato = await _contatoRepository.GetByIdAsync(contatoId);
            if (contato == null) return null;
                return new ContatoResponse
                {
                    Id = contato.Id,
                    Nome = contato.Nome,
                    Numero = contato.Numero,
                    GrupoId = contato.GrupoId,
                    OcorrenciaAtualId = contato.OcorrenciaAtualId
                };
        }

        public async Task<List<ContatoResponse>?> Execute(int userId, int groupId)
        {

            List<Contato>? contatos = await _contatoRepository.GetByGrupoAsync(groupId);
            List<ContatoResponse> res = new List<ContatoResponse>();
            foreach (var item in contatos)
            {
                res.Add(new ContatoResponse
                {
                    Id = item.Id,
                    Nome = item.Nome,
                    Numero = item.Numero,
                    GrupoId = item.GrupoId,
                    OcorrenciaAtualId = item.OcorrenciaAtualId
                });
            }
            if (res.Count > 0)
            {
                return res;
            }
            else
            {
                return null;
            }
        }
    }
}
