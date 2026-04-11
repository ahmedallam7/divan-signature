namespace UUNATEK.Domain.Entities;

public class PenUsageLog
{
    public Guid Id { get; set; }
    public int PenNumber { get; set; }
    public DateTime InstalledAt { get; set; }
    public DateTime? RemovedAt { get; set; }
    public double TotalDistanceMm { get; set; }
    public int TotalPrintJobs { get; set; }
    public int TotalStrokes { get; set; }
    public TimeSpan TotalDrawingTime { get; set; }
    public bool IsActive { get; set; }
    
    // Threshold tracking
    public bool WarningThresholdReached { get; set; }
    public bool CriticalThresholdReached { get; set; }
    public bool ReplacementThresholdReached { get; set; }
    
    // Navigation property
    public ICollection<RequestLog> RequestLogs { get; set; } = new List<RequestLog>();
}
