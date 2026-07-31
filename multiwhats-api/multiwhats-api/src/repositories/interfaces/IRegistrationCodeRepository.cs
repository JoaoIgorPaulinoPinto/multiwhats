using multiwhats_api.src.data.entities;

namespace multiwhats_api.src.repositories.interfaces;

public interface IRegistrationCodeRepository
{
    Task<RegistrationCode?> GetByCodeAsync(string code);
    Task<RegistrationCode?> GetTrackedByCodeAsync(string code);
    Task<RegistrationCode> AddAsync(RegistrationCode registrationCode);
    Task<RegistrationCode> UpdateAsync(RegistrationCode registrationCode);
    Task<List<RegistrationCode>> GetByCreatorAsync(int userId);
}
