using System.Globalization;
using UUNATRK.Application.Models;

namespace UUNATRK.Application.Services.Usage;

public class PenUsageCalculator : IPenUsageCalculator
{
    public PenUsageMetrics Calculate(List<string> gcode)
    {
        double totalDistance = 0;
        int strokeCount = 0;
        bool penIsDown = false;
        double currentX = 0, currentY = 0;
        
        foreach (var line in gcode)
        {
            var cmd = line.Trim();
            if (string.IsNullOrEmpty(cmd) || cmd.StartsWith(";"))
                continue;
            
            // Check if pen goes down (drawing starts)
            if (cmd.Contains("E8.0"))
            {
                penIsDown = true;
                strokeCount++;
                continue;
            }
            
            // Check if pen goes up (drawing stops)
            if (cmd.Contains("E0.0") || cmd.Contains("E4.0"))
            {
                penIsDown = false;
                continue;
            }
            
            // If pen is down and this is a G1 move, calculate distance
            if (penIsDown && cmd.StartsWith("G1"))
            {
                var (newX, newY) = ExtractXY(cmd, currentX, currentY);
                
                // Calculate Euclidean distance
                double dx = newX - currentX;
                double dy = newY - currentY;
                double distance = Math.Sqrt(dx * dx + dy * dy);
                
                totalDistance += distance;
                currentX = newX;
                currentY = newY;
            }
            // Track rapid moves (G0) for position updates
            else if (cmd.StartsWith("G0"))
            {
                var (newX, newY) = ExtractXY(cmd, currentX, currentY);
                currentX = newX;
                currentY = newY;
            }
        }
        
        // Calculate drawing duration (distance / feedrate)
        // Drawing feedrate is typically F5000 (5000 mm/min = 83.33 mm/s)
        double drawingFeedrateMMPerMin = 5000;
        double durationMinutes = totalDistance / drawingFeedrateMMPerMin;
        
        return new PenUsageMetrics
        {
            DrawingDistanceMm = totalDistance,
            StrokeCount = strokeCount,
            DrawingDuration = TimeSpan.FromMinutes(durationMinutes)
        };
    }
    
    private (double x, double y) ExtractXY(string gcode, double currentX, double currentY)
    {
        double x = currentX;
        double y = currentY;
        
        // Parse X and Y values from G-code like "G1 X10.5 Y20.3 F5000"
        var parts = gcode.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var part in parts)
        {
            if (part.StartsWith("X") && part.Length > 1)
            {
                if (double.TryParse(part.Substring(1), NumberStyles.Float, CultureInfo.InvariantCulture, out double parsedX))
                    x = parsedX;
            }
            else if (part.StartsWith("Y") && part.Length > 1)
            {
                if (double.TryParse(part.Substring(1), NumberStyles.Float, CultureInfo.InvariantCulture, out double parsedY))
                    y = parsedY;
            }
        }
        
        return (x, y);
    }
}
