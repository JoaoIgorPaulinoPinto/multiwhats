using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.usecases.interfaces.ContatoInterfaces;

namespace multiwhats_api.src.usecases.usecases.ContatoUseCases;

public class CriarContatoUseCase : ICriarContatoUseCase
{
    private readonly IContatoRepository _contatoRepository;

    public CriarContatoUseCase(IContatoRepository contatoRepository)
    {
        _contatoRepository = contatoRepository;
    }

    public async Task<ContatoResponse> Execute(CriarContatoRequest request, int usuarioId)
    {
        var existing = await _contatoRepository.GetByNumberAsync(request.Numero);
        if (existing != null)
            throw new InvalidOperationException("Já existe um contato com este número.");

        var contato = new Contato(
            request.Nome,
            request.Numero,
            request.GrupoId,
            request.OcorrenciaAtualId,
            usuarioId
        );

        var created = await _contatoRepository.AddAsync(contato);

        return new ContatoResponse
        {
            Id = created.Id,
            Nome = created.Nome,
            Numero = created.Numero,
            GrupoId = created.GrupoId,
            OcorrenciaAtualId = created.OcorrenciaAtualId
        };
    }

}
