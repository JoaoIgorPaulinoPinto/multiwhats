namespace multiwhats_api.src.usecases.interfaces.TaskInterfaces;

public interface IDeleteTaskUseCase
{
    Task<bool> Execute(int id);
}
