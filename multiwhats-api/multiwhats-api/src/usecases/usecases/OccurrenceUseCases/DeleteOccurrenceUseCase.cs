using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.OccurrenceInterfaces;

namespace multiwhats_api.src.usecases.usecases.OccurrenceUseCases;

public class DeleteOccurrenceUseCase : IDeleteOccurrenceUseCase
{
    private readonly IOccurrenceRepository _occurrenceRepository;
    private readonly UseCaseLogger _useCaseLogger;

    public DeleteOccurrenceUseCase(IOccurrenceRepository occurrenceRepository, UseCaseLogger useCaseLogger)
    {
        _occurrenceRepository = occurrenceRepository;
        _useCaseLogger = useCaseLogger;
    }

    public async Task<bool> Execute(int id)
    {
        var occurrence = await _occurrenceRepository.GetByIdAsync(id);
        if (occurrence == null)
            throw new KeyNotFoundException("Ocorrência não encontrada");

        var result = await _occurrenceRepository.DeleteAsync(id);

        await _useCaseLogger.LogAsync(
            action: "DeleteOccurrence",
            entityType: "Occurrence",
            entityId: id,
            description: $"Deleted occurrence #{id} (Title: {occurrence.Title})"
        );

        return result;
    }
}
