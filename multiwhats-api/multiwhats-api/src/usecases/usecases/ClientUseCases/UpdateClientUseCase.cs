using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.usecases.interfaces.ClientInterfaces;

namespace multiwhats_api.src.usecases.usecases.ClientUseCases;

public class UpdateClientUseCase : IUpdateClientUseCase
{
    private readonly IClientRepository _clientRepository;

    public UpdateClientUseCase(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public async Task<ClientResponse> Execute(int id, UpdateClientRequest request)
    {
        var client = await _clientRepository.GetByIdAsync(id);
        if (client == null)
            throw new KeyNotFoundException("Cliente não encontrado");

        client.Update(request.Name, request.MainPhoneNumber, request.Status);
        var updated = await _clientRepository.UpdateAsync(client);

        return new ClientResponse
        {
            Id = updated.Id,
            Name = updated.Name,
            MainPhoneNumber = updated.MainPhoneNumber,
            Status = updated.Status,
            ContactCount = updated.Contacts.Count,
            CreatedAt = updated.CreatedAt,
            LastUpdate = updated.LastUpdate
        };
    }
}
