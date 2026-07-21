using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.ClientInterfaces;

namespace multiwhats_api.src.usecases.usecases.ClientUseCases;

public class DeleteClientUseCase : IDeleteClientUseCase
{
    private readonly IClientRepository _clientRepository;
    private readonly UseCaseLogger _useCaseLogger;

    public DeleteClientUseCase(IClientRepository clientRepository, UseCaseLogger useCaseLogger)
    {
        _clientRepository = clientRepository;
        _useCaseLogger = useCaseLogger;
    }

    public async Task<bool> Execute(int id)
    {
        var client = await _clientRepository.GetByIdAsync(id);
        if (client == null)
            throw new KeyNotFoundException("Cliente não encontrado");

        var result = await _clientRepository.DeleteAsync(id);

        await _useCaseLogger.LogAsync(
            action: "DeleteClient",
            entityType: "Client",
            entityId: id,
            description: $"Deleted client #{id} (Name: {client.Name})"
        );

        return result;
    }
}
