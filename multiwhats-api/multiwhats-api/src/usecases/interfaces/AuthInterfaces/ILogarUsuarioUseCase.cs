using multiwhats_api.src.data.dtos.Responses;

namespace multiwhats_api.src.usecases.interfaces.AuthInterfaces;

public interface ILogarUsuarioUseCase
{
    Task<LoginResponse> Execute(multiwhats_api.src.data.dtos.Requests.LoginRequest request);
}
