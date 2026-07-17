using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;

namespace multiwhats_api.src.usecases.interfaces.ContatoInterfaces;

public interface ICriarContatoUseCase
{
    Task<ContatoResponse> Execute(CriarContatoRequest request, int usuarioId);
}
