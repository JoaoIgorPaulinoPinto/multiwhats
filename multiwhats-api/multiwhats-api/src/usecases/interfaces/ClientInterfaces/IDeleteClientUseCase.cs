namespace multiwhats_api.src.usecases.interfaces.ClientInterfaces;

public interface IDeleteClientUseCase
{
    Task<bool> Execute(int id);
}
