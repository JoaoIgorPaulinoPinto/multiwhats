using multiwhats_api.src.helpers;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.usecases.interfaces.ChatInterfaces;

namespace multiwhats_api.src.usecases.usecases.ChatUseCases;

public class MergeChatsUseCase : IMergeChatsUseCase
{
    private readonly IChatRepository _chatRepository;

    public MergeChatsUseCase(IChatRepository chatRepository)
    {
        _chatRepository = chatRepository;
    }

    public async Task<bool> Execute(string mergeJid, string toJid)
    {
        var source = await _chatRepository.GetByJidAsync(mergeJid)
            ?? await _chatRepository.GetByJidAsync(PhoneNumberHelper.Sanitize(mergeJid));

        var destination = await _chatRepository.GetByJidAsync(toJid)
            ?? await _chatRepository.GetByJidAsync(PhoneNumberHelper.Sanitize(toJid));

        if (source is null || destination is null)
            return false;

        return await _chatRepository.MergeChatAsync(source.Id, destination.Id);
    }
}

