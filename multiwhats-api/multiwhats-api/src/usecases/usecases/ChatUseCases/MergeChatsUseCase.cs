using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.usecases.interfaces.ChatInterfaces;

namespace multiwhats_api.src.usecases.interfaces.IChatUseCases
{
    public class MergeChatsUseCase : IMergeChatsUseCase
    {
        private readonly IChatRepository _chatRepository;

        public MergeChatsUseCase (IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        public async Task<bool> Merge(int mergeID, int toID)
        {

            
            Chat? chatToMerge = await _chatRepository.GetByIdAsync(mergeID);
            Chat? chatDestiny = await _chatRepository.GetByIdAsync(toID);

            if(chatToMerge != null && chatDestiny != null)
            {
                foreach (var item in chatToMerge.Messages)
                {
                    item.UpdateChatId(chatDestiny.Id);
                }
                await _chatRepository.UpdateAsync(chatDestiny);
                return true;
            }
            return false;
        }
    }
}
