using UUNATEK.Domain.Entities;

namespace UUNATRK.Application.Repositories;

public interface IPenUsageRepository
{
    Task<PenUsageLog?> GetActiveAsync();
    Task<PenUsageLog?> GetByIdAsync(Guid id);
    Task<PenUsageLog?> GetByPenNumberAsync(int penNumber);
    Task<List<PenUsageLog>> GetAllAsync();
    Task<List<PenUsageLog>> GetHistoryAsync(int limit = 100);
    Task<PenUsageLog> CreateAsync(PenUsageLog penUsageLog);
    Task UpdateAsync(PenUsageLog penUsageLog);
    Task<int> GetNextPenNumberAsync();
}
