namespace multiwhats_api.src.usecases.interfaces.ContactInterfaces;

public interface IDeleteContactUseCase
{
    Task<bool> Execute(int contactId, int userId);
}
