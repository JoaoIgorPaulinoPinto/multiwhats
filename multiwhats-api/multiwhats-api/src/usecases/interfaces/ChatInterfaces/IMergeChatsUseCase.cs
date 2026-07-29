namespace multiwhats_api.src.usecases.interfaces.ChatInterfaces;

public interface IMergeChatsUseCase
{
    Task<bool> Execute(string mergeJid, string toJid);
}
