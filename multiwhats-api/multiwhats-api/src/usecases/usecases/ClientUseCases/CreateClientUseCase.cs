using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.ClientInterfaces;

namespace multiwhats_api.src.usecases.usecases.ClientUseCases;

public class CreateClientUseCase : ICreateClientUseCase
{
    private readonly IClientRepository _clientRepository;
    private readonly UseCaseLogger _useCaseLogger;

    public CreateClientUseCase(IClientRepository clientRepository, UseCaseLogger useCaseLogger)
    {
        _clientRepository = clientRepository;
        _useCaseLogger = useCaseLogger;
    }

    public async Task<ClientResponse> Execute(CreateClientRequest request, int userId)
    {
        var client = new Client(request.Name, request.MainPhoneNumber);
        var created = await _clientRepository.AddAsync(client);

        await _useCaseLogger.LogAsync(
            action: "CreateClient",
            entityType: "Client",
            entityId: created.Id,
            description: $"Created client \"{created.Name}\" (Phone: {created.MainPhoneNumber}, Status: {created.Status})",
            explicitUserId: userId
        );

        return new ClientResponse
        {
            Id = created.Id,
            Name = created.Name,
            MainPhoneNumber = created.MainPhoneNumber,
            Status = created.Status,
            ContactCount = 0,
            CreatedAt = created.CreatedAt,
            LastUpdate = created.LastUpdate
        };
    }
}
