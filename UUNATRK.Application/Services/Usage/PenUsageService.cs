using Microsoft.Extensions.Options;
using UUNATEK.Domain.Entities;
using UUNATRK.Application.Models;
using UUNATRK.Application.Repositories;

namespace UUNATRK.Application.Services.Usage;

public class PenUsageService : IPenUsageService
{
    private readonly IPenUsageRepository _penUsageRepository;
    private readonly IRequestLogRepository _requestLogRepository;
    private readonly PenUsageSettings _settings;

    public PenUsageService(
        IPenUsageRepository penUsageRepository,
        IRequestLogRepository requestLogRepository,
        IOptions<PenUsageSettings> settings)
    {
        _penUsageRepository = penUsageRepository;
        _requestLogRepository = requestLogRepository;
        _settings = settings.Value;
    }

    public async Task<PenUsageLog?> GetActivePenAsync()
    {
        return await _penUsageRepository.GetActiveAsync();
    }

    public async Task<PenUsageLog> CreateNewPenAsync()
    {
        // Deactivate current pen if exists
        var currentPen = await _penUsageRepository.GetActiveAsync();
        if (currentPen != null)
        {
            currentPen.IsActive = false;
            currentPen.RemovedAt = DateTime.UtcNow;
            await _penUsageRepository.UpdateAsync(currentPen);
            Console.WriteLine($"Deactivated Pen #{currentPen.PenNumber}");
        }

        // Create new pen
        var nextPenNumber = await _penUsageRepository.GetNextPenNumberAsync();
        var newPen = new PenUsageLog
        {
            Id = Guid.NewGuid(),
            PenNumber = nextPenNumber,
            InstalledAt = DateTime.UtcNow,
            IsActive = true,
            TotalDistanceMm = 0,
            TotalPrintJobs = 0,
            TotalStrokes = 0,
            TotalDrawingTime = TimeSpan.Zero,
            WarningThresholdReached = false,
            CriticalThresholdReached = false,
            ReplacementThresholdReached = false
        };

        await _penUsageRepository.CreateAsync(newPen);
        Console.WriteLine($"Created new Pen #{newPen.PenNumber}");
        return newPen;
    }

    public async Task<(PenUsageLog pen, List<string> warnings)> AddUsageAsync(Guid? requestId, PenUsageMetrics metrics)
    {
        var pen = await GetActivePenAsync();
        if (pen == null)
        {
            // Auto-create first pen
            pen = await CreateNewPenAsync();
        }

        // Update pen totals
        pen.TotalDistanceMm += metrics.DrawingDistanceMm;
        pen.TotalPrintJobs++;
        pen.TotalStrokes += metrics.StrokeCount;
        pen.TotalDrawingTime += metrics.DrawingDuration;

        // Check thresholds
        double distanceKm = pen.TotalDistanceMm / 1000000.0;
        if (distanceKm >= _settings.ReplacementThresholdKm)
            pen.ReplacementThresholdReached = true;
        else if (distanceKm >= _settings.CriticalThresholdKm)
            pen.CriticalThresholdReached = true;
        else if (distanceKm >= _settings.WarningThresholdKm)
            pen.WarningThresholdReached = true;

        await _penUsageRepository.UpdateAsync(pen);

        // Update request log if requestId is provided
        if (requestId.HasValue)
        {
            var request = await _requestLogRepository.GetByRequestIdAsync(requestId.Value);
            if (request != null)
            {
                request.DrawingDistanceMm = metrics.DrawingDistanceMm;
                request.StrokeCount = metrics.StrokeCount;
                request.DrawingDuration = metrics.DrawingDuration;
                request.PenUsageLogId = pen.Id;
                await _requestLogRepository.UpdateAsync(request);
            }
        }

        // Generate warnings
        var warnings = CheckThresholds(pen, _settings);
        return (pen, warnings);
    }

    public async Task<PenUsageSummary> GetTotalUsageAsync()
    {
        var allPens = await _penUsageRepository.GetAllAsync();
        var currentPen = allPens.FirstOrDefault(p => p.IsActive);

        var summary = new PenUsageSummary
        {
            TotalPenCount = allPens.Count,
            CurrentPen = currentPen,
            TotalDistanceAllPensMm = allPens.Sum(p => p.TotalDistanceMm),
            TotalPrintJobs = allPens.Sum(p => p.TotalPrintJobs),
            PenHistory = allPens.OrderByDescending(p => p.InstalledAt).ToList(),
            ActiveWarnings = new List<string>()
        };

        // Add warnings for current pen
        if (currentPen != null)
        {
            var warnings = CheckThresholds(currentPen, _settings);
            summary.ActiveWarnings.AddRange(warnings);
        }

        return summary;
    }

    public async Task<List<PenUsageLog>> GetPenHistoryAsync()
    {
        return await _penUsageRepository.GetHistoryAsync();
    }

    public List<string> CheckThresholds(PenUsageLog pen, PenUsageSettings settings)
    {
        var warnings = new List<string>();
        double distanceKm = pen.TotalDistanceMm / 1000000.0;

        if (distanceKm >= settings.ReplacementThresholdKm)
        {
            warnings.Add($"🔴 CRITICAL: Pen #{pen.PenNumber} has exceeded replacement threshold ({distanceKm:F2}km / {settings.ReplacementThresholdKm:F1}km limit). Replace pen immediately!");
        }
        else if (distanceKm >= settings.CriticalThresholdKm)
        {
            warnings.Add($"🟠 WARNING: Pen #{pen.PenNumber} has exceeded critical threshold ({distanceKm:F2}km / {settings.CriticalThresholdKm:F1}km limit). Plan to replace pen soon.");
        }
        else if (distanceKm >= settings.WarningThresholdKm)
        {
            warnings.Add($"🟡 NOTICE: Pen #{pen.PenNumber} has exceeded warning threshold ({distanceKm:F2}km / {settings.WarningThresholdKm:F1}km limit). Monitor pen quality.");
        }

        return warnings;
    }
}
