using multiwhats_api.src.data.entities;

namespace multiwhats_api.src.repositories.interfaces;

public interface IGroupRepository
{
    Task<List<Group>> GetAllAsync();
    Task<Group?> GetByIdAsync(int id);
    Task<Group> AddAsync(Group group);
    Task<Group> UpdateAsync(Group group);
    Task<bool> DeleteAsync(int id);
    Task<List<Contact>> GetMembersAsync(int groupId);
}
