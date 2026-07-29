using multiwhats_api.src.data.entities;

namespace multiwhats_api.src.usecases.interfaces.ChatInterfaces
{
    public interface IMergeChatsUseCase
    {
        public Task<bool> Merge(int mergeID, int toID);
    }
}
