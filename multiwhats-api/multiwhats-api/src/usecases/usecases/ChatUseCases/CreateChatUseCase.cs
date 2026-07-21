using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.ChatInterfaces;

namespace multiwhats_api.src.usecases.usecases.ChatUseCases;

public class CreateChatUseCase : ICreateChatUseCase
{
    private readonly IChatRepository _chatRepository;
    private readonly UseCaseLogger _useCaseLogger;

    public CreateChatUseCase(IChatRepository chatRepository, UseCaseLogger useCaseLogger)
    {
        _chatRepository = chatRepository;
        _useCaseLogger = useCaseLogger;
    }

    public async Task<ChatResponse> Execute(CreateChatRequest request)
    {
        var existing = await _chatRepository.GetByJidAsync(request.Jid);
        if (existing != null)
            throw new InvalidOperationException("Já existe um chat com este JID.");

        var chat = new Chat(
            request.Jid,
            request.PhoneNumber,
            request.Name,
            request.ContactId,
            request.ClientId
        );

        var created = await _chatRepository.AddAsync(chat);

        await _useCaseLogger.LogAsync(
            action: "CreateChat",
            entityType: "Chat",
            entityId: created.Id,
            description: $"Created chat \"{created.Jid}\" (Name: {created.Name ?? "N/A"}, Phone: {created.PhoneNumber})"
        );

        return MapToResponse(created);
    }

    internal static ChatResponse MapToResponse(Chat chat)
    {
        return new ChatResponse
        {
            Id = chat.Id,
            Jid = chat.Jid,
            PhoneNumber = chat.PhoneNumber,
            Name = chat.Name ?? chat.Contact?.Name ?? chat.PhoneNumber,
            ContactId = chat.ContactId,
            ContactName = chat.Contact?.Name,
            ClientId = chat.ClientId,
            ClientName = chat.Client?.Name,
            LastMessageAt = chat.LastMessageAt,
            LastMessageBody = chat.LastMessageBody,
            AssignedToUserId = chat.AssignedToUserId,
            AssignedToUserName = chat.AssignedTo?.Name,
            CreatedByUserId = chat.CreatedByUserId,
            MessageCount = chat.Messages?.Count ?? 0,
            OccurrenceCount = chat.Occurrences?.Count ?? 0,
            CreatedAt = chat.CreatedAt,
            LastUpdate = chat.LastUpdate
        };
    }
}
