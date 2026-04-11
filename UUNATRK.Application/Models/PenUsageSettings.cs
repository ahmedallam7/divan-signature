namespace UUNATRK.Application.Models;

public class PenUsageSettings
{
    public double WarningThresholdKm { get; set; } = 5.0;
    public double CriticalThresholdKm { get; set; } = 10.0;
    public double ReplacementThresholdKm { get; set; } = 15.0;
    public bool TrackingEnabled { get; set; } = true;
}
