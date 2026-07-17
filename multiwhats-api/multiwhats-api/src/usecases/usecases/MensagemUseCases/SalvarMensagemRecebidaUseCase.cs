using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.data.dtos.Webhook;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.usecases.interfaces.ContatoInterfaces;
using multiwhats_api.src.usecases.interfaces.MensagemInterfaces;

namespace multiwhats_api.src.usecases.usecases.MensagemUseCases
{
    public class SalvarMensagemRecebidaUseCase : ISalvarMensagemRecebidaUseCase
    {
        private readonly IMensagemRepository _mensagemRepository;
        private readonly IPegarContatos _pegarContatos;
        private readonly ICriarContatoUseCase _criarContatoUseCase;

        public SalvarMensagemRecebidaUseCase(
            IMensagemRepository repository,
            IPegarContatos pegarContatoPorNumero,
            ICriarContatoUseCase criarContatoUseCase)
        {
            _criarContatoUseCase = criarContatoUseCase;
            _mensagemRepository = repository;
            _pegarContatos = pegarContatoPorNumero;
        }

        public async Task<bool> Execute(WhatsappMessageDto payload, int usuarioId)
        {
            ContatoResponse? contato = await _pegarContatos.Execute(payload.From, usuarioId);

            if (contato == null)
            {
                contato = await _criarContatoUseCase.Execute(new CriarContatoRequest
                    {
                        Nome = payload.NotifyName ?? payload.From,
                        Numero = payload.From,
                        GrupoId = null,
                        OcorrenciaAtualId = null, 
                },
                   usuarioId
                );
            }

            Mensagem msg = new Mensagem(
                payload.From,
                payload.Body,
                payload.Timestamp,
                payload.NotifyName,
                contato?.Id
            );

            return await _mensagemRepository.AdicionarAsync(msg);
        }
    }
}
