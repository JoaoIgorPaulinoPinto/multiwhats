using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;

namespace multiwhats_api.src.usecases.interfaces.AuthInterfaces;

public interface IRegisterUserUseCase
{
    Task<UserResponse> Execute(RegisterUserRequest request);
}
