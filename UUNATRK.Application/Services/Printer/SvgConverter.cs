using System.Xml.Linq;
using UUNATRK.Application.Models;

namespace UUNATRK.Application.Services.Printer
{
    public static class SvgConverter
    {
        public static List<string> ConvertToGCode(Stream svgStream, PrintRequest req)
        {
            var gcode = new List<string>();
            var doc = XDocument.Load(svgStream);
            var ns = doc.Root!.Name.Namespace;

            double offsetX = ParseMM(req.XPosition);
            double offsetY = ParseMM(req.YPosition);
            double scale = req.Scale * 0.30;

            // G92 sets origin at (9.0, -56.0)
            // User offsets are relative to this origin
            const double G92_X = 9.0;
            const double G92_Y = -56.0;

            bool penIsDown = false;

            double minX = 0, minY = 0, viewWidth = 200, viewHeight = 200;
            var viewBox = doc.Root.Attribute("viewBox")?.Value;
            if (!string.IsNullOrEmpty(viewBox))
            {
                var parts = viewBox.Split(' ');
                if (parts.Length == 4)
                {
                    minX = double.Parse(parts[0]);
                    minY = double.Parse(parts[1]);
                    viewWidth = double.Parse(parts[2]);
                    viewHeight = double.Parse(parts[3]);
                }
            }

            Console.WriteLine($"SVG ViewBox: {minX}, {minY}, {viewWidth}, {viewHeight}");
            Console.WriteLine($"Scale factor: {scale} (base 0.15 * user scale {req.Scale})");
            Console.WriteLine($"Max dimensions after scale: {viewWidth * scale}mm x {viewHeight * scale}mm");
            Console.WriteLine($"G92 origin: ({G92_X}, {G92_Y})");
            Console.WriteLine($"User offset: ({offsetX}, {offsetY})");
            Console.WriteLine($"First coord will be around: X={G92_X + offsetX}, Y={G92_Y + offsetY}");

            foreach (var path in doc.Descendants(ns + "path"))
            {
                var d = path.Attribute("d")?.Value;
                if (string.IsNullOrEmpty(d)) continue;

                foreach (var point in ParsePath(d))
                {
                    // Step 1: Translate to viewBox coordinates (remove offset)
                    double x = point.X - minX;
                    double y = point.Y - minY;

                    // Step 2: Apply axis inversions (before scaling)
                    if (req.InvertX)
                        x = viewWidth - x;

                    if (req.InvertY)
                        y = viewHeight - y;

                    // Step 3: Apply scale
                    x *= scale;
                    y *= scale;

                    // Step 4: Apply rotation
                    (double rx, double ry) = req.Rotation switch
                    {
                        90 => (y, -x),
                        180 => (-x, -y),
                        270 => (-y, x),
                        _ => (x, y)
                    };

                    // Step 5: Apply offset FROM the G92 origin
                    // Final coordinate = G92 origin + user offset + drawing position
                    double finalX = G92_X + offsetX + rx;
                    double finalY = G92_Y + offsetY + ry;

                    if (point.IsMove)
                    {
                        if (penIsDown)
                        {
                            // Partial lift between strokes -- E4.0, not E0.0
                            gcode.Add("G1 E4.0 F4000");
                            penIsDown = false;
                        }

                        // Fast move while pen is up -- F6000.0
                        gcode.Add($"G0 X{finalX:F3} Y{finalY:F3} F6000.0");

                        // Pen down
                        gcode.Add("G1 E8.0 F4000");
                        penIsDown = true;
                    }
                    else
                    {
                        // Draw -- F5000.0
                        gcode.Add($"G1 X{finalX:F3} Y{finalY:F3} F5000.0");
                    }
                }
            }

            // Final pen up -- full lift E0.0 (only here, never between strokes)
            if (penIsDown)
                gcode.Add("G1 E0.0 F4000");

            return gcode;
        }

        private static double ParseMM(string value) =>
            double.Parse(value.Replace("mm", "").Trim());

        private static List<PathPoint> ParsePath(string pathData)
        {
            var points = new List<PathPoint>();
            var tokens = pathData.Replace(",", " ").Split(' ', StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < tokens.Length; i++)
            {
                switch (tokens[i])
                {
                    case "M" when i + 2 < tokens.Length:
                        points.Add(new PathPoint(double.Parse(tokens[i + 1]), double.Parse(tokens[i + 2]), true));
                        i += 2;
                        break;
                    case "L" when i + 2 < tokens.Length:
                        points.Add(new PathPoint(double.Parse(tokens[i + 1]), double.Parse(tokens[i + 2]), false));
                        i += 2;
                        break;
                    case "C" when i + 6 < tokens.Length:
                        points.Add(new PathPoint(double.Parse(tokens[i + 5]), double.Parse(tokens[i + 6]), false));
                        i += 6;
                        break;
                }
            }

            return points;
        }
    }

    internal record PathPoint(double X, double Y, bool IsMove);
}
