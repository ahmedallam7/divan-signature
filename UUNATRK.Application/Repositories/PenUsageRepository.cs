using Microsoft.EntityFrameworkCore;
using UUNATEK.Domain.Entities;
using UUNATRK.Application.Data;

namespace UUNATRK.Application.Repositories;

public class PenUsageRepository : IPenUsageRepository
{
    private readonly ApplicationDbContext _context;

    public PenUsageRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PenUsageLog?> GetActiveAsync()
    {
        return await _context.PenUsageLogs
            .FirstOrDefaultAsync(p => p.IsActive);
    }

    public async Task<PenUsageLog?> GetByIdAsync(Guid id)
    {
        return await _context.PenUsageLogs
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<PenUsageLog?> GetByPenNumberAsync(int penNumber)
    {
        return await _context.PenUsageLogs
            .FirstOrDefaultAsync(p => p.PenNumber == penNumber);
    }

    public async Task<List<PenUsageLog>> GetAllAsync()
    {
        return await _context.PenUsageLogs
            .OrderBy(p => p.PenNumber)
            .ToListAsync();
    }

    public async Task<List<PenUsageLog>> GetHistoryAsync(int limit = 100)
    {
        return await _context.PenUsageLogs
            .OrderByDescending(p => p.InstalledAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<PenUsageLog> CreateAsync(PenUsageLog penUsageLog)
    {
        _context.PenUsageLogs.Add(penUsageLog);
        await _context.SaveChangesAsync();
        return penUsageLog;
    }

    public async Task UpdateAsync(PenUsageLog penUsageLog)
    {
        penUsageLog.InstalledAt = DateTime.SpecifyKind(penUsageLog.InstalledAt, DateTimeKind.Utc);
        _context.PenUsageLogs.Update(penUsageLog);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetNextPenNumberAsync()
    {
        var maxPenNumber = await _context.PenUsageLogs
            .MaxAsync(p => (int?)p.PenNumber) ?? 0;
        return maxPenNumber + 1;
    }
}
