namespace multiwhats_api.src.usecases.interfaces.OccurrenceInterfaces;

public interface IDeleteOccurrenceUseCase
{
    Task<bool> Execute(int id);
}
