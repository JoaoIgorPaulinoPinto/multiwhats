using multiwhats_api.src.data.dtos.Responses;

namespace multiwhats_api.src.usecases.interfaces.AuthInterfaces;

public interface IRegistrarUsuarioUseCase
{
    Task<UsuarioResponse> Execute(multiwhats_api.src.data.dtos.Requests.RegistrarUsuarioRequest request);
}
