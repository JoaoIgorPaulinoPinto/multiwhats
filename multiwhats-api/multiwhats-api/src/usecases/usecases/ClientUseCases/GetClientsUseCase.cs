using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.usecases.interfaces.ClientInterfaces;

namespace multiwhats_api.src.usecases.usecases.ClientUseCases;

public class GetClientsUseCase : IGetClientsUseCase
{
    private readonly IClientRepository _clientRepository;

    public GetClientsUseCase(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public async Task<List<ClientResponse>> ExecuteAll()
    {
        var clients = await _clientRepository.GetAllAsync();
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
