using UUNATEK.Domain.Entities;
using UUNATRK.Application.Models;

namespace UUNATRK.Application.Services.Usage;

public interface IPenUsageService
{
    Task<PenUsageLog?> GetActivePenAsync();
    Task<PenUsageLog> CreateNewPenAsync();
    Task<(PenUsageLog pen, List<string> warnings)> AddUsageAsync(Guid? requestId, PenUsageMetrics metrics);
    Task<PenUsageSummary> GetTotalUsageAsync();
    Task<List<PenUsageLog>> GetPenHistoryAsync();
    List<string> CheckThresholds(PenUsageLog pen, PenUsageSettings settings);
}
