using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.usecases.interfaces.OccurrenceInterfaces;

namespace multiwhats_api.src.usecases.usecases.OccurrenceUseCases;

public class DeleteOccurrenceUseCase : IDeleteOccurrenceUseCase
{
    private readonly IOccurrenceRepository _occurrenceRepository;

    public DeleteOccurrenceUseCase(IOccurrenceRepository occurrenceRepository)
    {
        _occurrenceRepository = occurrenceRepository;
    }

    public async Task<bool> Execute(int id)
    {
        var occurrence = await _occurrenceRepository.GetByIdAsync(id);
        if (occurrence == null)
            throw new KeyNotFoundException("Ocorrência não encontrada");

        return await _occurrenceRepository.DeleteAsync(id);
    }
}
