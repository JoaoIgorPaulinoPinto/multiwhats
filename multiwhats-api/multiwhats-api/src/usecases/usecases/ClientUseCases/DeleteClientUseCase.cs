using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.usecases.interfaces.ClientInterfaces;

namespace multiwhats_api.src.usecases.usecases.ClientUseCases;

public class DeleteClientUseCase : IDeleteClientUseCase
{
    private readonly IClientRepository _clientRepository;

    public DeleteClientUseCase(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public async Task<bool> Execute(int id)
    {
        var client = await _clientRepository.GetByIdAsync(id);
        if (client == null)
            throw new KeyNotFoundException("Cliente não encontrado");

        return await _clientRepository.DeleteAsync(id);
    }
}
