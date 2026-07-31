using multiwhats_api.src.data.entities;

namespace multiwhats_api.src.repositories.interfaces;

public interface ISystemParameterRepository
{
    Task<List<SystemParameter>> GetAllAsync();
    Task<SystemParameter?> GetByKeyAsync(string key);
    Task<SystemParameter> AddAsync(SystemParameter parameter);
    Task<SystemParameter> UpdateAsync(SystemParameter parameter);
    Task<bool> DeleteAsync(int id);
}
