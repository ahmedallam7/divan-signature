namespace UUNATRK.Application.Models;

public class PenUsageMetrics
{
    public double DrawingDistanceMm { get; set; }
    public int StrokeCount { get; set; }
    public TimeSpan DrawingDuration { get; set; }
    
    // Calculated property for convenience
    public double DrawingDistanceMeters => DrawingDistanceMm / 1000.0;
    public double DrawingDistanceKm => DrawingDistanceMm / 1000000.0;
}
