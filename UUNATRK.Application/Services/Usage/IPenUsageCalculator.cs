namespace UUNATRK.Application.Services.Usage;

public interface IPenUsageCalculator
{
    Models.PenUsageMetrics Calculate(List<string> gcode);
}
