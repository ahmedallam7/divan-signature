using UUNATEK.Domain.Entities;

namespace UUNATRK.Application.Models;

public class PenUsageSummary
{
    public int TotalPenCount { get; set; }
    public PenUsageLog? CurrentPen { get; set; }
    public double TotalDistanceAllPensMm { get; set; }
    public double TotalDistanceAllPensKm => TotalDistanceAllPensMm / 1000000.0;
    public int TotalPrintJobs { get; set; }
    public List<PenUsageLog> PenHistory { get; set; } = new();
    public List<string> ActiveWarnings { get; set; } = new();
}
