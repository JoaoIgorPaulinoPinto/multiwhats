using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.data.dtos.Responses;
using multiwhats_api.src.data.entities;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.AuthInterfaces;
using System.Security.Cryptography;

namespace multiwhats_api.src.usecases.usecases.AuthUseCases;

public class GenerateRegistrationCodeUseCase : IGenerateRegistrationCodeUseCase
{
    private readonly IRegistrationCodeRepository _registrationCodeRepository;
    private readonly UseCaseLogger _useCaseLogger;
    private readonly SystemConfigService _config;

    public GenerateRegistrationCodeUseCase(
        IRegistrationCodeRepository registrationCodeRepository,
        UseCaseLogger useCaseLogger,
        SystemConfigService config)
    {
        _registrationCodeRepository = registrationCodeRepository;
        _useCaseLogger = useCaseLogger;
        _config = config;
    }

    public async Task<List<RegistrationCodeResponse>> Execute(GenerateRegistrationCodeRequest request, int userId)
    {
        var expiryHours = await _config.GetIntAsync("Auth:RegistrationCodeExpiryHours", 48);
        var responses = new List<RegistrationCodeResponse>();

        for (int i = 0; i < request.Quantity; i++)
        {
            var codeValue = await GenerateUniqueCodeAsync();
            var registrationCode = new RegistrationCode(codeValue, userId, expiryHours);
            var created = await _registrationCodeRepository.AddAsync(registrationCode);

            responses.Add(new RegistrationCodeResponse
            {
                Id = created.Id,
                Code = created.Code,
                IsUsed = created.IsUsed,
                UsedByUserId = created.UsedByUserId,
                ExpiresAt = created.ExpiresAt,
                CreatedAt = created.CreatedAt
            });
        }

        await _useCaseLogger.LogAsync(
            action: "GenerateRegistrationCodes",
            entityType: "RegistrationCode",
            entityId: null,
            description: $"Generated {request.Quantity} registration code(s) (user #{userId})"
        );

        return responses;
    }

    private static string GenerateCode()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(4));
    }

    private async Task<string> GenerateUniqueCodeAsync()
    {
        for (int attempt = 0; attempt < 10; attempt++)
        {
            var code = GenerateCode();
            if (!await _registrationCodeRepository.ExistsAsync(code))
                return code;
        }

        throw new InvalidOperationException("Não foi possível gerar um código único. Tente novamente.");
    }
}
