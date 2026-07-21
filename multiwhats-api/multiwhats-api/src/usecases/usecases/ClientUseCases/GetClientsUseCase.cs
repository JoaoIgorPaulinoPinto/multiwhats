using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.ClientInterfaces;

namespace multiwhats_api.src.usecases.usecases.ClientUseCases;

public class GetClientsUseCase : IGetClientsUseCase
{
    private readonly IClientRepository _clientRepository;
    private readonly UseCaseLogger _useCaseLogger;

    public GetClientsUseCase(IClientRepository clientRepository, UseCaseLogger useCaseLogger)
    {
        _clientRepository = clientRepository;
        _useCaseLogger = useCaseLogger;
    }

    public async Task<List<ClientResponse>> ExecuteAll()
    {
        var clients = await _clientRepository.GetAllAsync();

        await _useCaseLogger.LogAsync(
            action: "GetClients",
            entityType: "Client",
            entityId: null,
            description: $"Listed all clients (count: {clients.Count})"
        );

        return clients.Select(c => new ClientResponse
        {
            Id = c.Id,
            Name = c.Name,
            MainPhoneNumber = c.MainPhoneNumber,
            Status = c.Status,
            ContactCount = c.Contacts.Count,
            CreatedAt = c.CreatedAt,
            LastUpdate = c.LastUpdate
        }).ToList();
    }

    public async Task<ClientResponse?> ExecuteById(int id)
    {
        var client = await _clientRepository.GetByIdAsync(id);
        if (client == null) return null;

        await _useCaseLogger.LogAsync(
            action: "GetClient",
            entityType: "Client",
            entityId: id,
            description: $"Retrieved client #{id} (Name: {client.Name})"
        );

        return new ClientResponse
        {
            Id = client.Id,
            Name = client.Name,
            MainPhoneNumber = client.MainPhoneNumber,
            Status = client.Status,
            ContactCount = client.Contacts.Count,
            CreatedAt = client.CreatedAt,
            LastUpdate = client.LastUpdate
        };
    }
}
